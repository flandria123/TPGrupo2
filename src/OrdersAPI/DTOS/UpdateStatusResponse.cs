namespace OrdersAPI.DTOS
{
    public record UpdateStatusResponse(
        Guid Id,
        string Estado,
        DateTime FechaActualizacion
    );
}
