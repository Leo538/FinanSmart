namespace FinanSmart.Api.Common.Exceptions;

public class NoCurrentCreditRateException : Exception
{
    public NoCurrentCreditRateException() : base("No active current rate was found for this credit type.")
    {
    }
}

