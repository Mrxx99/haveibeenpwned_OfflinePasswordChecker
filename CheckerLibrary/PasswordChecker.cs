using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckerLibrary
{
    public static class PasswordChecker
    {
        public static int FindPassword(string hash, string[] lines)
        {
            foreach (var line in lines)
            {
                if (line.Contains(hash))
                {
                    var numbersFoundString = line.Split(' ').ElementAtOrDefault(1);
                    int.TryParse(numbersFoundString, out int numbers);

                    return numbers;
                }
            }

            return 0;
        }

        public static async Task<int> FindPassword(string hash, string filePath, IProgress<double> progressReporter, CancellationToken cancelToken = default)
        {
            hash = hash.ToUpperInvariant();

            cancelToken.ThrowIfCancellationRequested();

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                byte[] buffer = new byte[0x1000];
                int numRead;
                string previousLastLine = "";
                string[] stringSeparators = { "\n" };
                int lineLength = 0;
                int lastLineIndex = 0;
                double progress;
                long fileLenth = sourceStream.Length;

                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancelToken)) != 0)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    string text = Encoding.ASCII.GetString(buffer, 0, numRead);
                    string[] lines = text.Split(stringSeparators, StringSplitOptions.None);

                    lines[0] = string.Concat(previousLastLine, lines[0]);

                    lineLength = lines.Length;
                    lastLineIndex = lineLength - 1;

                    for (int i = 0; i < lineLength; i++)
                    {
                        string line = lines[i];

                        if (i == lastLineIndex)
                        {
                            if (line.Contains("\r"))
                            {
                                previousLastLine = "";
                            }
                            else
                            {
                                previousLastLine = line;
                            }

                            break;
                        }
                        if (line.Contains("\r") && line.Contains(hash))
                        {
                            string occurrencesString = line.Split(':')[1].Replace("\r", "");
                            return int.Parse(occurrencesString);
                        }
                    }

                    progress = (100 * sourceStream.Position) / fileLenth;
                    progressReporter.Report(progress);
                }

                cancelToken.ThrowIfCancellationRequested();
            }

            return 0;
        }
    }
}
