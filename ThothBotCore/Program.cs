using System;
using System.Linq;

namespace ThothBotCore
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Any() && args[0] == "--version") Console.WriteLine("0.0.1");
            Console.WriteLine("Hello World!");
        }
    }
}
