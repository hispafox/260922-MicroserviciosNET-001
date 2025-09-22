using MassTransit;
using Integration.Contracts;
using Payments.Application;
using Analytics.Application;
using Delivery.Application;
using Notifications.Application;

public class PedidoCreadoConsumer : IConsumer<PedidoCreado>
{
    private readonly PaymentsService _payments;
    private readonly AnalyticsService _analytics;
    private readonly NotificationsService _notify;
    public PedidoCreadoConsumer(PaymentsService payments, AnalyticsService analytics, NotificationsService notify)
    { _payments = payments; _analytics = analytics; _notify = notify; }

    public async Task Consume(ConsumeContext<PedidoCreado> ctx)
    {
        await _analytics.OnOrderCreated(ctx.Message.OrderId, ctx.Message.Total, ctx.CancellationToken);
        await _notify.AddAsync(ctx.Message.OrderId, "INFO", $"Pedido creado por {ctx.Message.Total}", ctx.CancellationToken);
        var approved = await _payments.AuthorizeAsync(ctx.Message.OrderId, ctx.Message.Total, ctx.CancellationToken);
        if (approved) await ctx.Publish(new PagoAprobado(ctx.Message.OrderId));
        else await ctx.Publish(new PagoRechazado(ctx.Message.OrderId));
    }
}

public class PagoAprobadoConsumer : IConsumer<PagoAprobado>
{
    private readonly DeliveryService _delivery;
    private readonly AnalyticsService _analytics;
    private readonly NotificationsService _notify;
    public PagoAprobadoConsumer(DeliveryService delivery, AnalyticsService analytics, NotificationsService notify)
    { _delivery = delivery; _analytics = analytics; _notify = notify; }

    public async Task Consume(ConsumeContext<PagoAprobado> ctx)
    {
        await _delivery.AssignAsync(ctx.Message.OrderId, ctx.CancellationToken);
        await _analytics.OnPaymentUpdated(ctx.Message.OrderId, true, ctx.CancellationToken);
        await _notify.AddAsync(ctx.Message.OrderId, "SUCCESS", "Pago aprobado", ctx.CancellationToken);
        await ctx.Publish(new PedidoListoParaReparto(ctx.Message.OrderId));
    }
}

public class PagoRechazadoConsumer : IConsumer<PagoRechazado>
{
    private readonly AnalyticsService _analytics;
    private readonly NotificationsService _notify;
    public PagoRechazadoConsumer(AnalyticsService analytics, NotificationsService notify)
    { _analytics = analytics; _notify = notify; }

    public async Task Consume(ConsumeContext<PagoRechazado> ctx)
    {
        await _analytics.OnPaymentUpdated(ctx.Message.OrderId, false, ctx.CancellationToken);
        await _notify.AddAsync(ctx.Message.OrderId, "ERROR", "Pago rechazado", ctx.CancellationToken);
    }
}
