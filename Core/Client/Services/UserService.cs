using Core.Client.Abstrations;
using Core.Client.Models;
using Core.Common;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Core.Client.Services;

public class UserService(IUserRepository userRepository, ILogger<UserService> logger) : IUserService
{
    public async Task<Result<User>> GetUserById(string id)
    {
        var user = await userRepository.GetUserById(id);
        return user;
    }

    public async Task<Result<List<User>>> GetUsers()
    {
        logger.LogInformation("Hello, world!");
        
        return  new List<User>
        {
            new User("novo usuario","<EMAIL>","aaa","","","", DateTime.Now, DateTime.Now)
        };
        
        var users = userRepository.GetUsers();
        
        //return await users;
    }

    public async Task<Result<User>> MockCreateUserAsync(Models.User user)
    {
        try
        {
            // Simular criação do usuário
            // Aqui você faria a chamada para o repositório

            // aqui o user é emcapsulado de forma implicita no retorno para camda API.
            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}