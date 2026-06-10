namespace OrdersAPI.DTOS
{
    /// <summary>
    /// Objeto que confirma la actualización del estado de una orden.
    /// </summary>
    public record UpdateStatusResponse
    {
        /// <summary>Identificador único de la orden modificada.</summary>
        /// <example>f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
        public required Guid Id { get; init; }

        /// <summary>El nuevo estado aplicado.</summary>
        /// <example>Confirmada</example>
        public required string Estado { get; init; }

        /// <summary>Fecha y hora en que se actualizó el estado.</summary>
        /// <example>2024-03-10T12:00:00Z</example>
        public required DateTime FechaActualizacion { get; init; }
    }
}
