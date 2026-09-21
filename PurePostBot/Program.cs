using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PurePostBot.utils;
using PurePostBot.handlers;
using Microsoft.Extensions.Options;
using PurePostBot.services;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;

namespace PurePostBot
{
    class Program
    {
        static async Task Main()
        {
            // SQLite init
            SQLitePCL.Batteries.Init();

            // DI container
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // Read token from env
                    string token = Environment.GetEnvironmentVariable(
                        "Bot__Token", EnvironmentVariableTarget.User) 
                    ?? throw new ArgumentNullException("Env:Bot__Token is null");

                    // Bot options
                    services
                        .AddOptions<BotOptions>()
                        .Configure(options =>
                        {
                            options.Token = token;
                        })
                        .Validate(
                            options =>
                            !string.IsNullOrEmpty(options.Token),
                            "Bot:Token is required")
                        .ValidateOnStart();

                    // Telegram bot client
                    services.AddSingleton<ITelegramBotClient>(sp =>
                    {
                        var options = sp
                            .GetRequiredService<IOptions<BotOptions>>()
                            .Value;

                        return new TelegramBotClient(options.Token);
                    });

                    // TgHost
                    services.AddSingleton<TgHost>();

                    // Db context 
                    services.AddSingleton<AppDbContext>();

                    // Cache
                    services.AddMemoryCache();

                    // Messages cache
                    services.AddSingleton(sp =>
                        new PostingCacheService(
                            sp.GetRequiredService<IMemoryCache>(),
                            TimeSpan.FromMinutes(20)));

                    // User service
                    services.AddSingleton(sp =>
                        new UserService(
                            sp.GetRequiredService<AppDbContext>(),
                            sp.GetRequiredService<IMemoryCache>(),
                            TimeSpan.FromMinutes(8)));

                    // Media group service
                    services.AddSingleton<MediaGroupService>();

                    // Options service
                    services.AddSingleton<OptionsService>();

                    // Group id service
                    services.AddSingleton<GroupIdService>();

                    // Caption service
                    services.AddSingleton<CaptionService>();

                    // Post edit service
                    services.AddSingleton<PostEditService>();

                    // Start handler
                    services.AddSingleton<StartHandler>();

                    // Help handler
                    services.AddSingleton<HelpHandler>();

                    // Options handler
                    services.AddSingleton<OptionsHandler>();

                    // Message handler
                    services.AddSingleton<DefaultHandler>();

                    // Bot
                    services.AddSingleton<Bot>();
                }).Build();

            // Bot init
            var bot = host.Services.GetRequiredService<Bot>();
            await bot.InitAsync();

            Console.Read();
        }
    }
}