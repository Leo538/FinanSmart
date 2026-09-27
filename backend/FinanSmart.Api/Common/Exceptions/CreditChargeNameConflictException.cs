namespace FinanSmart.Api.Common.Exceptions;

public class CreditChargeNameConflictException : Exception
{
    public CreditChargeNameConflictException() : base("An active charge with this name already exists for this credit type.")
    {
    }
}

