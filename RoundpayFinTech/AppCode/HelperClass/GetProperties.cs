using System;
using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class GetProperties
    {
        public static GetProperties o => instance.Value;
        private static Lazy<GetProperties> instance = new Lazy<GetProperties>(() => new GetProperties());
        private GetProperties()
        {

        }
        public List<string> GetPropertiesNameOfClass(object pObject)
        {
            List<string> propertyList = new List<string>();
            if (pObject != null)
            {
                foreach (var prop in pObject.GetType().GetProperties())
                {
                    propertyList.Add(prop.Name);
                }
            }
            return propertyList;
        }

        public List<string> GetPropertiesNameOfClassRecursively(object pObject, List<string> propertyList)
        {
            if (propertyList == null)
            {
                propertyList = new List<string>();
            }
            if (pObject != null)
            {
                foreach (var prop in pObject.GetType().GetProperties())
                {
                    object propValue = prop.GetValue(pObject, null);
                    if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                    {
                        propertyList.Add(prop.Name);
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
                    {
                        System.Collections.IEnumerable ienumerable = (System.Collections.IEnumerable)propValue;
                        foreach (object item in ienumerable)
                        {
                            GetPropertiesNameOfClassRecursively(item, propertyList);
                        }
                        
                    }
                    else
                    {
                        GetPropertiesNameOfClassRecursively(propValue, propertyList);
                    }
                }
            }
            
            return propertyList;
        }
    }
}
