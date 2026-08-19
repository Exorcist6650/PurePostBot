using Services;
using SqlDB;
using DataManagement;
using TgBot;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens.Experimental;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utils;
using Handlers;

namespace MyApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN", EnvironmentVariableTarget.User);

            if (token is null)
            {
                ConsoleLogger.Log("Token is null"); // Log
                return;
            }

            // SQLite init
            SQLitePCL.Batteries.Init();

            // DI container
            var host = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
            {
                services.AddMemoryCache();

                // TgHost
                services.AddSingleton((sp) =>
                    new TgHost(token));

                // DB
                services.AddSingleton<SqlDb>((sp) =>
                    new SqlDb(new DbOptions
                    {
                        ConnectionString = $"Data Source = botdata.db"
                    }));

                // User repository
                services.AddSingleton<UserRepository>();

                // User repository cache
                services.AddSingleton((sp) =>
                    new UserRepositoryCache(
                        sp.GetRequiredService<UserRepository>(),
                        sp.GetRequiredService<IMemoryCache>(),
                        TimeSpan.FromMinutes(8)));

                // Messages cache
                services.AddSingleton((sp) =>
                    new PostingMessagesCache(
                        sp.GetRequiredService<IMemoryCache>(),
                        TimeSpan.FromMinutes(20)));

                // User service
                services.AddSingleton<UserService>();

                // Media group service
                services.AddSingleton<MediaGroupService>();

                // Options service
                services.AddSingleton((sp) =>
                    new OptionsService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>()));

                // Group id service
                services.AddSingleton((sp) =>
                    new GroupIdService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>(),
                        sp.GetRequiredService<OptionsService>()));

                // Caption service
                services.AddSingleton((sp) =>
                    new CaptionService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>(),
                        sp.GetRequiredService<OptionsService>()));

                // Post edit service
                services.AddSingleton((sp) =>
                    new PostEditService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>(),
                        sp.GetRequiredService<PostingMessagesCache>()));

                // Start handler
                services.AddSingleton((sp) =>
                    new StartHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>()));

                // Help handler
                services.AddSingleton((sp) =>
                    new HelpHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot));

                // Options handler
                services.AddSingleton((sp) =>
                    new OptionsHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>()));

                // Message handler
                services.AddSingleton((sp) =>
                    new DefaultHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<MediaGroupService>(),
                        sp.GetRequiredService<PostEditService>()));

                // Bot
                services.AddSingleton<Bot>();
            }).Build();

            using(var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SqlDb>();

                // Table query
                string query = @"
                CREATE TABLE IF NOT EXISTS Users(
                    Id BIGINT NOT NULL PRIMARY KEY, 
                    GroupId BIGINT NULL,
                    Caption TEXT NULL,
                    IsChangingGroupId BOOLEAN NOT NULL,
                    IsChangingCaption BOOLEAN NOT NULL
                    
                );";

                await db.InitAsync(query);

                // Bot init
                var bot = scope.ServiceProvider.GetRequiredService<Bot>();
                await bot.InitAsync();
            }

            Console.Read();
        }
    }
}