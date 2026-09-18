namespace FinanSmart.Api.Common.Exceptions;
public class InvestmentProductNameConflictException() : Exception("An active investment product with this name already exists.");
public class InvestmentProductNotFoundException() : Exception("Investment product was not found.");
public class InvestmentRateOverlapException() : Exception("The investment rate overlaps an existing active rate.");
