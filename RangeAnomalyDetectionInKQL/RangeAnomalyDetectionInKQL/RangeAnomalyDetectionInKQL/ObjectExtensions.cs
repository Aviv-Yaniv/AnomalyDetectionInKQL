using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RangeAnomalyDetectionInKQL
{
    public static class ObjectExtensions
    {
        public static T DictionaryToObject<T>(IDictionary<string, object> dict) where T : class
        {
            Type type = typeof(T);
            T result = (T)Activator.CreateInstance(type);
            foreach (var item in dict)
            {
                type.GetProperty(item.Key).SetValue(result, item.Value, null);
            }
            return result;
        }

        public static IDictionary<string, object> ObjectToDictionary<T>(T item) where T : class
        {
            Type myObjectType = item.GetType();
            IDictionary<string, object> dict = new Dictionary<string, object>();
            var indexer = new object[0];
            PropertyInfo[] properties = myObjectType.GetProperties();
            foreach (var info in properties)
            {
                var value = info.GetValue(item, indexer);
                dict.Add(info.Name, value);
            }
            return dict;
        }
    }
}
