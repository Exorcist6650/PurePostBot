using System;
using PurePostBot;

namespace MyApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN", EnvironmentVariableTarget.User);
            
            if (token != null)
            {
                var host = new Host(token);
                await host.Start();
                Console.WriteLine(host.Me);
            }

            Console.Read();
        }
    }
}