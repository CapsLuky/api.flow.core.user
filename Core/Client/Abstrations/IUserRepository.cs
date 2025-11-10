using Core.Client.Models;
using Core.Common;

namespace Core.Client.Abstrations;

public interface IUserRepository
{
    Task<Result<User?>> GetUserById(string id);
    Task<Result<List<User>>> GetUsers();

}