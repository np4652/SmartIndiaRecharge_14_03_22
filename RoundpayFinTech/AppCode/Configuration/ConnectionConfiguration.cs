using System.IO;
using Fintech.AppCode.DB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
namespace Fintech.AppCode.Configuration
{
    public class ConnectionConfiguration: IConnectionConfiguration
    {
        public readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public ConnectionConfiguration(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public string GetConnectionString(int Type = 0, string MM_YYYY = "")
        {
            switch (Type)
            {
                case 1:
                    return Configuration["ConnectionStrings:DBCon_Month"];
                case 2:
                    return Configuration["ConnectionStrings:DBCon_Old"].Replace("MM_YYYY", MM_YYYY);
                default:
                    return Configuration["ConnectionStrings:DBCon"];
            }
        }
    }
}
