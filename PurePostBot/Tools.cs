using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurePostBot
{
    public enum LogStatus : byte
    {
        Message,
        Warning,
        Error,
    }

    static class ConsoleLogger
    {
        public static void Log(string text, LogStatus status = LogStatus.Message)
        {
            switch (status)
            {
                // Message standard
                case LogStatus.Message:
                    Console.WriteLine($"{text} | {DateTime.UtcNow}");
                    break;

                // Warning yellow color
                case LogStatus.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{text} | {DateTime.UtcNow}");
                    break;

                // Error red color
                case LogStatus.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{text} | {DateTime.UtcNow}");
                    break;
            }

            Console.ResetColor(); // Reset console color
        }
    }
}
