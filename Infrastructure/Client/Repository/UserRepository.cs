
using Core.Client.Abstrations;
using Core.Common;
using Core.Generic.Models;
using Infrastructure.Statics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Client.Repository
{
    public class UserRepository(
        ILogger<UserRepository> logger,
        EnvironmentMongoUrl mongoUrl
        ) : IUserRepository
    {
        private IMongoCollection<Core.Client.Models.User> GetUserCollection()
        {
            var connectionString = mongoUrl.UrlMongoDb;
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(Database.MongoDbSupplierHub);
            return database.GetCollection<Core.Client.Models.User>("users");
        }

        public async Task<Result<Core.Client.Models.User?>> GetUserById(string id)
        {
            try
            {
                var collection = GetUserCollection();
                var filter = Builders<Core.Client.Models.User>.Filter.Eq(u => u.Id, id);
                var user = await collection.Find(filter).FirstOrDefaultAsync();
            
                return user;
            }
            catch (ArgumentException e)
            {
                logger.LogError(e, "Erro de argumento ao buscar usuário por ID: {Id}", id);
                throw;
            }
            catch (MongoException e)
            {
                logger.LogError(e, "Erro do MongoDB ao buscar usuário por ID: {Id}", id);
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Erro inesperado ao buscar usuário por ID: {Id}", id);
                throw;
            }
        }
    
        public async Task<Result<List<Core.Client.Models.User>>> GetUsers()
        {
            try
            {
                var collection = GetUserCollection();
                var users = await collection.Find(FilterDefinition<Core.Client.Models.User>.Empty).ToListAsync();
            
                return users;
            }
            catch (ArgumentException e)
            {
                logger.LogError(e, "Erro de argumento ao buscar todos os usuários");
                throw;
            }
            catch (MongoException e)
            {
                logger.LogError(e, "Erro do MongoDB ao buscar todos os usuários");
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Erro inesperado ao buscar todos os usuários");
                throw;
            }

        }
    }
}
