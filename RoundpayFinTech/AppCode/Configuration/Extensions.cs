using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fintech.AppCode.Configuration
{
    public static class Extensions
    {
        public static bool In<TItem>(this TItem source, Func<TItem, TItem, bool> comparer, IEnumerable<TItem> items)
        {
            return items.Any(item => comparer(source, item));
        }

        public static bool In<TItem, T>(this TItem source, Func<TItem, T> selector, IEnumerable<TItem> items)
        {
            return items.Select(selector).Contains(selector(source));
        }

        public static bool In<T>(this T source, IEnumerable<T> items)
        {
            return items.Contains(source);
        }

        public static bool In<TItem>(this TItem source, Func<TItem, TItem, bool> comparer, params TItem[] items)
        {
            return source.In(comparer, (IEnumerable<TItem>)items);
        }

        public static bool In<TItem, T>(this TItem source, Func<TItem, T> selector, params TItem[] items)
        {
            return source.In(selector, (IEnumerable<TItem>)items);
        }

        public static bool In<T>(this T source, params T[] items)
        {
            return source.In((IEnumerable<T>)items);
        }
        public static string UppercaseWords(this string value)
        {
            char[] array = value.ToCharArray();
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
    }
    public static class HttpRequestExtensions
    {

        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(request.Body, encoding))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Retrieves the raw body as a byte array from the Request.Body stream
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetRawBodyBytesAsync(this HttpRequest request)
        {
            using (var ms = new MemoryStream(2048))
            {
                await request.Body.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
