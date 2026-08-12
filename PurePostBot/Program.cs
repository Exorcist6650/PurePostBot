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
                services.AddSingleton<TgHost>((sp) =>
                    new TgHost(token));

                // DB
                services.AddSingleton<SqlDb>((sp) =>
                    new SqlDb(new DbOptions
                    {
                        ConnectionString = $"Data Source = botdata.db"
                    }));

                // User repository
                services.AddSingleton<UserRepository>((sp) =>
                    new UserRepository(sp.GetRequiredService<SqlDb>()));

                // User repository cache
                services.AddSingleton<UserRepositoryCache>((sp) =>
                    new UserRepositoryCache(
                        sp.GetRequiredService<UserRepository>(),
                        sp.GetRequiredService<IMemoryCache>(),
                        TimeSpan.FromMinutes(8)));

                // Messages cache
                services.AddSingleton<PostingMessagesCache>((sp) =>
                    new PostingMessagesCache(
                        sp.GetRequiredService<IMemoryCache>(),
                        TimeSpan.FromMinutes(20)));

                // User service
                services.AddSingleton<UserService>((sp) =>
                    new UserService(
                        sp.GetRequiredService<UserRepository>(),
                        sp.GetRequiredService<UserRepositoryCache>()));

                // Media group service
                services.AddSingleton<MediaGroupService>();

                // Options service
                services.AddSingleton<OptionsService>((sp) =>
                    new OptionsService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>()));

                // Group id service
                services.AddSingleton<GroupIdService>((sp) =>
                    new GroupIdService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>(),
                        sp.GetRequiredService<OptionsService>()));

                // Post edit service
                services.AddSingleton<PostEditService>((sp) =>
                    new PostEditService(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>(),
                        sp.GetRequiredService<PostingMessagesCache>()));

                // Start handler
                services.AddSingleton<StartHandler>((sp) =>
                    new StartHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>()));

                // Help handler
                services.AddSingleton<HelpHandler>((sp) =>
                    new HelpHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot));

                // Options handler
                services.AddSingleton<OptionsHandler>((sp) =>
                    new OptionsHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<UserService>()));

                // Message handler
                services.AddSingleton<DefaultHandler>((sp) =>
                    new DefaultHandler(
                        sp.GetRequiredService<TgHost>().TelegramBot,
                        sp.GetRequiredService<MediaGroupService>(),
                        sp.GetRequiredService<PostEditService>()));

                // Bot
                services.AddSingleton<Bot>((sp) =>
                    new Bot(
                        sp.GetRequiredService<TgHost>(),
                        sp.GetRequiredService<UserService>(),
                        sp.GetRequiredService<MediaGroupService>(),
                        sp.GetRequiredService<OptionsService>(),
                        sp.GetRequiredService<GroupIdService>(),
                        sp.GetRequiredService<PostingMessagesCache>(),
                        sp.GetRequiredService<PostEditService>(),
                        sp.GetRequiredService<StartHandler>(),
                        sp.GetRequiredService<HelpHandler>(),
                        sp.GetRequiredService<OptionsHandler>(),
                        sp.GetRequiredService<DefaultHandler>()));
            }).Build();

            using(var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SqlDb>();
                await db.InitAsync();

                // Bot init
                var bot = scope.ServiceProvider.GetRequiredService<Bot>();
                await bot.InitAsync();
            }

            Console.Read();
        }
    }
}