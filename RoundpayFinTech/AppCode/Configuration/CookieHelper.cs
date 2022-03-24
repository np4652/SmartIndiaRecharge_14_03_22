using System;
using Microsoft.AspNetCore.Http;
namespace Fintech.AppCode.Configuration
{
    public class CookieHelper
    {
        IHttpContextAccessor _accessor;
        public CookieHelper(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
        public string Get(string Key)
        {
            return _accessor.HttpContext.Request.Cookies[Key];
        }
        public void Set(string Key, string Value, DateTime? expiryDate)
        {
            CookieOptions options = new CookieOptions
            {
                Expires = expiryDate.HasValue ? expiryDate : DateTime.Now
            };
            _accessor.HttpContext.Response.Cookies.Append(Key, Value, options);
        }
        public void Remove(string Key)
        {
            _accessor.HttpContext.Response.Cookies.Delete(Key);
        }
    }
}
