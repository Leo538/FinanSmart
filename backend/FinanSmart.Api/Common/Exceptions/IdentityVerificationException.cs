namespace FinanSmart.Api.Common.Exceptions;
public class IdentityVerificationException(string message,int statusCode=400):Exception(message){public int StatusCode{get;}=statusCode;}
