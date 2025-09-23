using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Menu.Domain.Exceptions;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (title, status) = context.Exception switch
        {
            ValidationException => ("Errores de validación", 400),
            EntityNotFoundException => ("No encontrado", 404),
            InvalidPriceException => ("Precio inválido", 400),
            DomainException => ("Error de dominio", 400),
            UnauthorizedAccessException => ("No autorizado", 401),
            InvalidOperationException => ("Operación inválida", 400),
            _ => ("Error interno", 500)
        };

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = context.Exception.Message
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
