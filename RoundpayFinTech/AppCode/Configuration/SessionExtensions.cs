using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fintech.AppCode.Configuration
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session,string key, object value) {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetObjectFromJson<T>(this ISession session, string Key) {
            var v = session.GetString(Key);
            return v == null ? default(T) : JsonConvert.DeserializeObject<T>(v);
        }
    }
}
