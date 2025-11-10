using Core.Client.Models;
using Core.Common;

namespace Core.Client.Services;

public interface IUserService
{
    Task<Result<User>> GetUserById(string id);
    Task<Result<List<User>>> GetUsers();
    Task<Result<User>> MockCreateUserAsync(Models.User user);

}