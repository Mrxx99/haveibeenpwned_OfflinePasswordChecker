using CheckerLibrary;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Start();
            Console.ReadLine();
        }

        public static async Task Start()
        {
            string checkAgainResponse = "";

            string filepath = null;

            using var cancellationTokenSource = new CancellationTokenSource();

            ConsoleCancelEventHandler cancelHandler = (sender, args) => cancellationTokenSource.Cancel();

            Console.CancelKeyPress += cancelHandler;

            do
            {
                Console.WriteLine("your password:");

                var hash = HashCalculator.CalculateHashString(ReadPasswordFromConsole());

                Console.WriteLine();
                Console.WriteLine($"the hash is: {hash}");

                if (filepath == null)
                {
                    Console.WriteLine("file path to hash file:");
                    filepath = Console.ReadLine();
                }

                if (File.Exists(filepath))
                {
                    filepath = Path.GetFullPath(filepath);

                    Console.WriteLine(filepath);

                    Console.WriteLine("search started");
                    var time = new Stopwatch();
                    time.Start();

                    await AnsiConsole.Progress()
                        .StartAsync(async ctx =>
                        {
                            // Define tasks
                            var progressTask = ctx.AddTask("[green]Searching for password hash[/]"); ;

                            var progressHandler = new Progress<double>(value => progressTask.Value = progressTask.Value);

                            int occurrences = 0;

                            try
                            {
                                occurrences = await PasswordChecker.FindPassword(hash, filepath, progressHandler, cancellationTokenSource.Token);
                            }
                            catch (OperationCanceledException ex)
                            {
                                Console.WriteLine("Canceled.");
                                return;
                            }

                            time.Stop();
                            Console.WriteLine($"Search took {time.Elapsed}");

                            if (occurrences > 0)
                            {
                                Console.WriteLine($"Bad news: Your password was found inside the database. This password has been seen {occurrences} times before");
                            }
                            else
                            {
                                Console.WriteLine("Good news: No occurences found");
                            }
                        });

                    do
                    {
                        Console.WriteLine("Check another password? (Y/N)");
                        checkAgainResponse = Console.ReadLine();
                    }
                    while (!checkAgainResponse.Equals("Y", StringComparison.CurrentCultureIgnoreCase)
                        && !checkAgainResponse.Equals("N", StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    Console.WriteLine("filepath invaild or not found");
                }
            } while (checkAgainResponse.Equals("Y", StringComparison.CurrentCultureIgnoreCase));

            Console.CancelKeyPress -= cancelHandler;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static string ReadPasswordFromConsole()
        {
            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter [green]password[/]")
                    .PromptStyle("red")
                    .Secret());

            return password;
        }
    }
}
