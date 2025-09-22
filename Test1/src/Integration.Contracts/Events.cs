namespace Integration.Contracts;

public record PedidoCreado(Guid OrderId, decimal Total);
public record PagoAprobado(Guid OrderId);
public record PagoRechazado(Guid OrderId);
public record PedidoListoParaReparto(Guid OrderId);
public record AnaliticaActualizada(Guid OrderId, decimal Total, string PaymentStatus);
