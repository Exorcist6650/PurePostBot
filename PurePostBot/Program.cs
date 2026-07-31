using TgBot;

namespace MyApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN", EnvironmentVariableTarget.User);

            if (token != null)
            {
                var bot = new Bot(new Host(token), new Services.MediaGroupService());
                await bot.Init();

                Console.Read();
            }
            else
                Console.WriteLine("Token is null");
        }
    }
}