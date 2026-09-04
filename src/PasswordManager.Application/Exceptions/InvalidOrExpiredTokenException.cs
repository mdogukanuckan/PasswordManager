namespace PasswordManager.Application.Exceptions;

public class InvalidOrExpiredTokenException : Exception
{
    public InvalidOrExpiredTokenException() : base("Token is invalid or expired."){}
}