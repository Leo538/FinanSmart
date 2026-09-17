namespace FinanSmart.Api.Common.Exceptions;

public class CreditTypeNameConflictException : Exception
{
    public CreditTypeNameConflictException() : base("A credit type with this name already exists.")
    {
    }
}

