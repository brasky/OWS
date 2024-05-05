using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using OWSShared.Interfaces;
using OWSShared.Middleware;
using Serilog;

namespace OWSPublicAPI
{
    /// <summary>
    /// OWS Public API Program
    /// </summary>
    /// <remarks>
    /// The program class.
    /// </remarks>
    public class Program
    {
        /// <summary>
        /// OWS Public API Main
        /// </summary>
        /// <remarks>
        /// The program entry point.
        /// </remarks>
        public static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                CreateHostBuilder(args)
                    .UseOrleansClient(client =>
                    {
                        client.UseLocalhostClustering();
                        client.AddOutgoingGrainCallFilter<CustomerIdClientFilter>();
                    })
                    .Build().Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// OWS Public API CreateHostBuilder
        /// </summary>
        /// <remarks>
        /// Configure the web host.
        /// </remarks>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, serviceProvider, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .ReadFrom.Services(serviceProvider);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                    builder.AddSerilog();
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    public class CustomerIdClientFilter : IOutgoingGrainCallFilter
    {
        public CustomerIdClientFilter(IHeaderCustomerGUID customerGuid)
        {
            _customerGuid = customerGuid;
        }

        IHeaderCustomerGUID _customerGuid;

        public async Task Invoke(IOutgoingGrainCallContext context)
        {

            RequestContext.Set("CustomerId", _customerGuid.CustomerGUID);
            await context.Invoke();

        }
    }
}
