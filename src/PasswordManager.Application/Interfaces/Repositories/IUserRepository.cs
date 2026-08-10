using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
}