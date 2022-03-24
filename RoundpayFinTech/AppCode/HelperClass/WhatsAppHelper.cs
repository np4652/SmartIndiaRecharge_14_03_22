using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.HelperClass
{
    public static  class WhatsAppHelper
    {
        public static string getValueFromUrlString(string urlString, string Key)
        {
            var sString = urlString.Split('?')[1];
            var lst = sString.Split('&').ToList();
            var from = lst.Where(x => x.Contains(Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return from.Split('=')[1];
        }
    }
   
}
