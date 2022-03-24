using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RoundpayFinTech.AppCode.Model;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class JWTTokenAuth_ML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        
        public JWTTokenAuth_ML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile(_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json");
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public string GenerateJwtToken(JWTReqUsers jWTReqUsers)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            string tKey = Configuration["JWT:Key"];
            if (string.IsNullOrEmpty(tKey))
                return "Wrong Key";
            var key = Encoding.ASCII.GetBytes(tKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", jWTReqUsers.UserID.ToString()),
                    new Claim("token", jWTReqUsers.UserToken)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
