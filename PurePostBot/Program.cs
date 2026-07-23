using System;
using PurePostBot;

namespace MyApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN", EnvironmentVariableTarget.User);
            
            if (token != null)
            {
                var bot = new TgBot(new Host(token));
                await bot.Init();
            }

            Console.Read();
        }
    }
}