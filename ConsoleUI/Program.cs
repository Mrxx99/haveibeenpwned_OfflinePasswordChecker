using CheckerLibrary;
using System;

namespace ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("your password:");
            var input = Console.ReadLine();
            var hash = HashCalculator.CalculateHashString(input);
            Console.WriteLine($"the hash is: {hash}");
            Console.ReadLine();
        }
    }
}
