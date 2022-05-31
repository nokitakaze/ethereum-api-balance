namespace EthereumAPIBalance.WebApi.Services;

public class APIException : Exception
{
    public int ErrorCode { get; private set; }

    public APIException()
    {
    }

    public APIException(string error) : base(error)
    {
    }

    public APIException(string error, int code) : base(error)
    {
        ErrorCode = code;
    }
}