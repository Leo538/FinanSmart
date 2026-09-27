namespace FinanSmart.Api.Common.Exceptions;

public class CreditRateOverlapException : Exception
{
    public CreditRateOverlapException() : base("The effective period overlaps an active credit rate.")
    {
    }
}

