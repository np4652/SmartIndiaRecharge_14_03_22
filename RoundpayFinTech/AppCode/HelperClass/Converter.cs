using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Fintech.AppCode.HelperClass
{
    public class ConverterHelper
    {
        private const string FixedReplacer = "AAAAAAAAAA";
        public static ConverterHelper O => instance.Value;
        private static Lazy<ConverterHelper> instance = new Lazy<ConverterHelper>(() => new ConverterHelper());
        private ConverterHelper() { }
        public string NumberToText(int number)
        {
            if (number == 0) return "Zero";

            if (number == -2147483648) return "Minus Two Hundred and Fourteen Crore Seventy Four Lakh Eighty Three Thousand Six Hundred Forty Eight";

            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (number < 0)
            {
                sb.Append("Minus ");
                number = -number;
            }

            string[] words0 = {"" ,"One ", "Two ", "Three ", "Four ",
"Five " ,"Six ", "Seven ", "Eight ", "Nine "};

            string[] words1 = {"Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ",
"Fifteen ","Sixteen ","Seventeen ","Eighteen ", "Nineteen "};

            string[] words2 = {"Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ",
"Seventy ","Eighty ", "Ninety "};

            string[] words3 = { "Thousand ", "Lakh ", "Crore " };

            num[0] = number % 1000; // units 
            num[1] = number / 1000;
            num[2] = number / 100000;
            num[1] = num[1] - 100 * num[2]; // thousands 
            num[3] = number / 10000000; // crores 
            num[2] = num[2] - 100 * num[3]; // lakhs 

            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }


            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;

                u = num[i] % 10; // ones 
                t = num[i] / 10;
                h = num[i] / 100; // hundreds 
                t = t - 10 * h; // tens 

                if (h > 0) sb.Append(words0[h] + "Hundred ");

                if (u > 0 || t > 0)
                {
                    if (h > 0 || i == 0) sb.Append(" ");

                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }

                if (i != 0) sb.Append(words3[i - 1]);

            }
            return sb.ToString().TrimEnd();
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public List<SplitAmount> SplitAmounts(int RAmt, int min, int max)
        {
            List<SplitAmount> splitAmounts = new List<SplitAmount>();
            if (RAmt < 1)
            {
                return splitAmounts;
            }
            min = min > 0 ? min : 100;
            max = max > 0 ? max : 5000;

            SplitAmount split;

            if (RAmt <= min || min > max)
            {
                split = new SplitAmount
                {
                    Amount = RAmt
                };
                splitAmounts.Add(split);
                return splitAmounts;
            }
            int div = RAmt / max;
            int mod = RAmt % max;

            if (mod > min || mod == 0)
            {
                for (int i = 0; i < div; i++)
                {
                    split = new SplitAmount
                    {
                        Amount = max
                    };
                    splitAmounts.Add(split);
                }
                if (mod > 0)
                {
                    split = new SplitAmount
                    {
                        Amount = mod
                    };
                    splitAmounts.Add(split);
                }
            }
            else
            {
                int LastAmt = max + mod;
                for (int i = 0; i < div - 1; i++)
                {
                    split = new SplitAmount
                    {
                        Amount = max
                    };
                    splitAmounts.Add(split);
                }
                split = new SplitAmount
                {
                    Amount = LastAmt - min
                };
                splitAmounts.Add(split);
                split = new SplitAmount
                {
                    Amount = min
                };
                splitAmounts.Add(split);
            }
            return splitAmounts;
        }

        public string Generate10To20UniqueID(string PreVal, string PostVal)
        {
            /**
             * Based on asumption PostVal will not be trimed
             * **/
            PreVal = (PreVal ?? "");
            PostVal = (PostVal ?? "");
            var preLength = PreVal.Length;
            var postLength = PostVal.Length;
            if ((preLength + postLength) > 19)
            {
                if (preLength > 9)
                {
                    PreVal = PreVal.Substring(0, 9);
                }
                else if ((preLength - 2) > 0)
                {
                    PreVal = PreVal.Substring(0, preLength - 2);
                }
                return PreVal + FixedReplacer.Substring(0, 1) + PostVal;
            }
            else
            {
                var rep = 20 - (preLength + postLength);
                if (rep <= 10)
                {
                    return PreVal + FixedReplacer.Substring(0, rep) + PostVal;
                }
                return PreVal + FixedReplacer + PostVal;
            }
        }
        public string ConvertImagebase64(string path)
        {
            var imgstring = string.Empty;
            try
            {
                if (File.Exists(path))
                {
                    var memory = new MemoryStream();
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        stream.CopyTo(memory);
                    }
                    memory.Position = 0;
                    var bts = memory.ToArray();
                    imgstring = Convert.ToBase64String(bts);
                }
            }
            catch (Exception ex)
            {
            }

            return imgstring;
        }

        public string ReplaceSpaceWithSingle(string s)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(s ?? string.Empty, " ");
        }
        public static byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public string[] seprateMobileNos(string text)
        {
            //var exp = new Regex(@"(\(?[0-9]{10}\)?)?\-?[0-9]{3}\-?[0-9]{4}",RegexOptions.IgnoreCase);
            var exp = new Regex(@"((\+*)((0[ -]*)*|((91 )*))((\d{12})+|(\d{10})+))|\d{5}([- ]*)\d{6}", RegexOptions.IgnoreCase);
            MatchCollection matchList = exp.Matches(text);
            return matchList.Select(x => x.Value).ToArray();
        }
    }

    public class SplitAmount
    {
        public int Amount { get; set; }
    }
}

