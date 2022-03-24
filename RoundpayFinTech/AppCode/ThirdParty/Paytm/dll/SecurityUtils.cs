using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace paytm.util
{
    internal class SecurityUtils
    {
        public static string createCheckSumString(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return "";
            }
            MessageConsole.WriteLine();
            MessageConsole.WriteLine("Input Dict::::");
            SecurityUtils.printDictionary(parameters);
            SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            MessageConsole.WriteLine();
            MessageConsole.WriteLine("Sorted Dict::::");
            SecurityUtils.printSortedDictionary(sortedDictionary);
            StringBuilder stringBuilder = new StringBuilder("");
            foreach (KeyValuePair<string, string> current in sortedDictionary)
            {
                string text = current.Value;
                if (text == null || text.Trim().Equals("NULL"))
                {
                    text = "";
                }
                stringBuilder.Append(text).Append("|");
            }
            return stringBuilder.ToString();
        }

        public static string getHashedString(string inputValue)
        {
            byte[] bytesFromString = StringUtils.getBytesFromString(inputValue);
            SHA256Managed sHA256Managed = new SHA256Managed();
            byte[] value = sHA256Managed.ComputeHash(bytesFromString);
            string text = BitConverter.ToString(value);
            return text.Replace("-", "").ToLower();
        }

        private static void printDictionary(Dictionary<string, string> dict)
        {
            if (dict == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> current in dict)
            {
                MessageConsole.WriteLine("{0}, {1}", current.Key, current.Value);
            }
        }

        private static void printSortedDictionary(SortedDictionary<string, string> dict)
        {
            if (dict == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> current in dict)
            {
                MessageConsole.WriteLine("{0}, {1}", current.Key, current.Value);
            }
        }
    }
}
