namespace DiAvto.Web.Features.Marketplaces.Che168;

public sealed class Che168ApiException : Exception
{
    public Che168ApiException(string message) : base(message)
    {
    }

    public Che168ApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
