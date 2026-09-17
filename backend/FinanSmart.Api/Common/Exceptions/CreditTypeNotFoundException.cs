namespace FinanSmart.Api.Common.Exceptions;

public class CreditTypeNotFoundException : Exception
{
    public CreditTypeNotFoundException() : base("The credit type was not found.")
    {
    }
}

