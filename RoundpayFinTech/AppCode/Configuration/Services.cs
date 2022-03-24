using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using System;

namespace RoundpayFinTech.AppCode.Configuration
{
    public static class Services
    {
        public static void AddService(this IServiceCollection service)
        {
            service.AddScoped<IUnitOfWork, UnitOfWork>();
            service.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fintech APIs",
                    Version = "v1.1",                   
                    Contact = new OpenApiContact
                    {
                        Name = "Amit Singh",
                        Email = "np4652@gmail.com",
                        Url = new Uri("https://github.com/np4652")
                    }
                });
            });
        }
    }
}
