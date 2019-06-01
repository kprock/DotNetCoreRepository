using DotNetCoreRepository.DAL;
using DotNetCoreRepository.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(
            this IServiceCollection services)
        {
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            // HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.AddScoped(it => it.GetService<IUrlHelperFactory>()
                .GetUrlHelper(it.GetService<IActionContextAccessor>().ActionContext));

            // Add SAP data services
            services.AddScoped<ISAPDataService, SAPDataService>();
            services.AddScoped<IDatabaseFactorySAP, DatabaseFactorySAP>();
            services.AddScoped<IDIAPI_Services, DIAPI_Services>();
            services.AddScoped<IDIAPI_Services_FBA, DIAPI_Services_FBA>();

            // Add repository services
            services.AddTransient<ISalesRepository, SalesRepository>();

            // FBS services
            services.AddTransient<IAmazonOrderLogRepository, AmazonOrderLogRepository>();

            // factories and units of work
            services.AddScoped<IDatabaseFactory, DatabaseFactory>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
