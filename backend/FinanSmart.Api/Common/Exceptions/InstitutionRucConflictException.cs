namespace FinanSmart.Api.Common.Exceptions;

public class InstitutionRucConflictException : Exception
{
    public InstitutionRucConflictException() : base("An institution with this RUC already exists.")
    {
    }
}

