using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;



namespace RoundpayFinTech.AppCode.HelperClass
{
    public class ApplicationSettingSerializecs
    {

        public static string SerializeStaticClass(System.Type _Type,bool ssBoolOnly=true)
        {
            if (ssBoolOnly)
            {             
                var TypeBlob = _Type.GetFields().Where(y => y.FieldType == typeof(bool)).ToDictionary(x => x.Name, x => x.GetValue(null));
                return JsonConvert.SerializeObject(TypeBlob);
            }
            else
            {
                var TypeBlob = _Type.GetFields().ToDictionary(x => x.Name, x => x.GetValue(null));
                return JsonConvert.SerializeObject(TypeBlob);
            }
        }
    }
}
