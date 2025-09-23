using Menu.Domain.Abstractions;
using Menu.Domain.Entities;
using Menu.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Persistence;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly MenuDbContext _db;
    public EfRepository(MenuDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public void Remove(T e) => _db.Set<T>().Remove(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();

    public async Task<PagedResult<T>> GetPagedAsync(int page, int size, string? filter = null, CancellationToken ct = default)
    {
        var query = _db.Set<T>().AsNoTracking().AsQueryable();
        // Filtrado dinámico por Name y Description si existen
        if (!string.IsNullOrEmpty(filter))
        {
            var type = typeof(T);
            var nameProp = type.GetProperty("Name");
            var descProp = type.GetProperty("Description");
            if (nameProp != null || descProp != null)
            {
                query = query.Where(e =>
                    (nameProp != null && EF.Functions.Like(EF.Property<string>(e, "Name"), $"%{filter}%")) ||
                    (descProp != null && EF.Functions.Like(EF.Property<string>(e, "Description"), $"%{filter}%"))
                );
            }
        }
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = size
        };
    }

    public async Task<PagedResult<T>> GetPagedKeysetAsync(int? lastId, int size, string? filter = null, CancellationToken ct = default)
    {
        var query = _db.Set<T>().AsNoTracking().AsQueryable();
        var type = typeof(T);
        var idProp = type.GetProperty("Id");
        if (idProp == null)
            throw new InvalidOperationException($"Keyset paging requires an 'Id' property on {type.Name}");

        // Filtrado dinámico por Name y Description si existen
        if (!string.IsNullOrEmpty(filter))
        {
            var nameProp = type.GetProperty("Name");
            var descProp = type.GetProperty("Description");
            if (nameProp != null || descProp != null)
            {
                query = query.Where(e =>
                    (nameProp != null && EF.Functions.Like(EF.Property<string>(e, "Name"), $"%{filter}%")) ||
                    (descProp != null && EF.Functions.Like(EF.Property<string>(e, "Description"), $"%{filter}%"))
                );
            }
        }

        // Keyset/Seek: solo los elementos con Id > lastId
        if (lastId.HasValue)
        {
            query = query.Where(e => EF.Property<int>(e, "Id") > lastId.Value);
        }
        query = query.OrderBy(e => EF.Property<int>(e, "Id"));

        var items = await query.Take(size).ToListAsync(ct);
        var totalCount = await query.CountAsync(ct); // Opcional: totalCount puede ser solo los que cumplen filtro y lastId
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = lastId.HasValue ? (lastId.Value / size) + 1 : 1,
            PageSize = size
        };
    }

    public async Task<PagedTokenResult<T>> GetPagedTokenAsync(string? token, int size, string? filter = null, CancellationToken ct = default)
    {
        var query = _db.Set<T>().AsNoTracking().AsQueryable();
        var type = typeof(T);
        var idProp = type.GetProperty("Id");
        if (idProp == null)
            throw new InvalidOperationException($"Token paging requires an 'Id' property on {type.Name}");

        // Filtrado dinámico por Name y Description si existen
        if (!string.IsNullOrEmpty(filter))
        {
            var nameProp = type.GetProperty("Name");
            var descProp = type.GetProperty("Description");
            if (nameProp != null || descProp != null)
            {
                query = query.Where(e =>
                    (nameProp != null && EF.Functions.Like(EF.Property<string>(e, "Name"), $"%{filter}%")) ||
                    (descProp != null && EF.Functions.Like(EF.Property<string>(e, "Description"), $"%{filter}%"))
                );
            }
        }

        int? lastId = null;
        if (!string.IsNullOrEmpty(token))
        {
            // Decodifica el token (base64 de Id)
            try
            {
                var idStr = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(token));
                if (int.TryParse(idStr, out var id))
                    lastId = id;
            }
            catch { /* token inválido, ignora y empieza desde el principio */ }
        }
        if (lastId.HasValue)
        {
            query = query.Where(e => EF.Property<int>(e, "Id") > lastId.Value);
        }
        query = query.OrderBy(e => EF.Property<int>(e, "Id"));

        var items = await query.Take(size).ToListAsync(ct);
        string? nextToken = null;
        if (items.Count > 0)
        {
            // Genera el token para la siguiente página (base64 del último Id)
            var nextId = (int)idProp.GetValue(items.Last())!;
            nextToken = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(nextId.ToString()));
        }
        return new PagedTokenResult<T>
        {
            Items = items,
            NextToken = nextToken,
            PageSize = size
        };
    }
}

public class MenuItemRepository : EfRepository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(MenuDbContext db) : base(db) {}

    public async Task<MenuItem> GetByIdAsync(object id, CancellationToken ct = default)
    {
        var item = await _db.Items
            .TagWith("GetByIdAsync - MenuItem Query")
            .FirstOrDefaultAsync(x => x.Id.Equals(id), ct);
        if (item == null)
            throw new EntityNotFoundException(nameof(MenuItem), id);
        return item;
    }
    public async Task AddAsync(MenuItem e, CancellationToken ct = default) => await base.AddAsync(e, ct);
    public void Update(MenuItem e) => base.Update(e);
    public void Remove(MenuItem e) => base.Remove(e);
    public IQueryable<MenuItem> Query() => base.Query();

    public async Task<IEnumerable<MenuItem>> GetAvailableAsync()
    {
        return await _db.Items
            .TagWith("GetAvailableAsync - MenuItem Query")
            .Where(x => x.Stock > 0)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _db.Items
            .TagWith("ExistsByNameAsync - MenuItem Query")
            .AnyAsync(x => x.Name == name);
    }

    public async Task<List<MenuItem>> GetActiveMenuItemsAsync(int page, int size)
    {
        return await _db.Items
            .TagWith("GetActiveMenuItems - Pagination Query")
            .AsNoTracking()
            .Where(x => x.Stock > 0)
            .OrderBy(x => x.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }
}
