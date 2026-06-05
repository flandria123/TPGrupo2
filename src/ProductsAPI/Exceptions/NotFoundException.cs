namespace ProductsAPI.Exceptions
{
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;

    }
}