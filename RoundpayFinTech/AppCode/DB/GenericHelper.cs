using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace System.Reflection
{
    public static class GenericHelper
    {
        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, Convert.ChangeType(dr[prop.Name],prop.PropertyType), null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        public static List<T> BindList<T>(DataTable dt)
        {
            var fields = typeof(T).GetFields();
            List<T> lst = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                var ob = Activator.CreateInstance<T>();
                foreach (var fieldInfo in fields)
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (fieldInfo.Name == dc.ColumnName)
                        {
                            object value = Convert.ChangeType(dr[dc.ColumnName],fieldInfo.FieldType);
                            fieldInfo.SetValue(ob, value);
                            break;
                        }
                    }
                }
                lst.Add(ob);
            }
            return lst;
        }
        public static DataTable ToDataTable<T>(this List<T> iList)
        {
            DataTable dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection =
                TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);


                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        //    private static object GetValue(object ob, Type targetType)
        //    {
        //        if (targetType == null)
        //        {
        //            return null;
        //        }
        //        else if (targetType == typeof(String))
        //        {
        //            return ob + "";
        //        }
        //        else if (targetType == typeof(int))
        //        {
        //            int i = 0;
        //            int.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(short))
        //        {
        //            short i = 0;
        //            short.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(long))
        //        {
        //            long i = 0;
        //            long.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(ushort))
        //        {
        //            ushort i = 0;
        //            ushort.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(uint))
        //        {
        //            uint i = 0;
        //            uint.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(ulong))
        //        {
        //            ulong i = 0;
        //            ulong.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(double))
        //        {
        //            double i = 0;
        //            double.TryParse(ob + "", out i);
        //            return i;
        //        }
        //        else if (targetType == typeof(DateTime))
        //        {
        //            // do the parsing here...
        //        }
        //        else if (targetType == typeof(bool))
        //        {
        //            // do the parsing here...
        //        }
        //        else if (targetType == typeof(decimal))
        //        {
        //            // do the parsing here...
        //        }
        //        else if (targetType == typeof(float))
        //        {
        //            // do the parsing here...
        //        }
        //        else if (targetType == typeof(byte))
        //        {
        //            // do the parsing here...
        //        }
        //        else if (targetType == typeof(sbyte))
        //        {
        //            // do the parsing here...
        //        }

        //return ob;
        //    }

    }
}
