namespace Utils
{
    public enum ELogStatus : byte
    {
        Message,
        Warning,
        Error,
    }

    static class ConsoleLogger
    {
        public static void Log(string text, ELogStatus status = ELogStatus.Message)
        {
            switch (status)
            {
                // Message standard
                case ELogStatus.Message:
                    Console.WriteLine($"{text} | {DateTime.UtcNow}");
                    break;

                // Warning yellow color
                case ELogStatus.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{text} | {DateTime.UtcNow}");
                    break;

                // Error red color
                case ELogStatus.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{text} | {DateTime.UtcNow}");
                    break;
            }

            Console.ResetColor(); // Reset console color
        }
    }
}
