using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.HelperClass
{
    public class DynamicHelper
    {
        private static Lazy<DynamicHelper> Instance = new Lazy<DynamicHelper>(() => new DynamicHelper());
        public static DynamicHelper O => Instance.Value;
        private DynamicHelper() { }
        public List<object> GetKeyValuePairs(string jsonString, string root, bool IsRootListType)
        {
            var list = new List<object>();
            Dictionary<string, dynamic> dyn = new Dictionary<string, dynamic>();
            try
            {
                var d = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);
                if (IsRootListType)
                {
                    dyn = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(d[root][0]));
                }
                else
                {
                    dyn = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(d[root]));
                }
               
                foreach (var item in dyn)
                {
                    list.Add(item);
                }
            }
            catch (Exception ex)
            {
            }
            return list;
        }
    }
}
