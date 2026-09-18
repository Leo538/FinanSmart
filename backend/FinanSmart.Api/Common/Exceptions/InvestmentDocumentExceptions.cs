namespace FinanSmart.Api.Common.Exceptions;

public class InvestmentDocumentValidationException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
