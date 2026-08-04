using Services;
using SqlDB;
using DataManagement;
using TgBot;
using Microsoft.Extensions.Caching.Memory;

namespace MyApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN", EnvironmentVariableTarget.User);

            if (token != null)
            {
                // Db instance
                SQLitePCL.Batteries.Init();
                var db = new SqlDb(new DbOptions
                    { ConnectionString = $"Data Source = botdata.db" }
                );
                await db.InitAsync();

                // Bot instance
                var bot = new Bot(
                    new Host(token), 
                    new UserService(
                        new UserRepository(db), 
                        new UserRepositoryCache(
                            new UserRepository(db), 
                            new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(8)
                        )
                    ),
                    new MediaGroupService()
                );
                await bot.Init();


                Console.Read();
            }
            else
                Console.WriteLine("Token is null");
        }
    }
}