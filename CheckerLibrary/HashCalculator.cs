using System;
using System.Security.Cryptography;

namespace CheckerLibrary
{
    public static class HashCalculator
    {
        public static string CalculateHashString(string input)
        {
            var bytes = CalculateHashBytes(input);

            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        public static byte[] CalculateHashBytes(string input)
        {
            var asciiEncoder = new System.Text.ASCIIEncoding();

            var bytes = asciiEncoder.GetBytes(input);

            return CalculateHash(bytes);
        }

        private static byte[] CalculateHash(byte[] input)
        {
            var prov = SHA1.Create();
            return prov.ComputeHash(input);
        }
    }
}
