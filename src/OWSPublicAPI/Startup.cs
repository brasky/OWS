using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using OWSShared.Implementations;
using OWSShared.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

namespace OWSPublicAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            //InstanceLauncherStartup.AddInstanceLauncherStartupOptions(Configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("./temp/DataProtection-Keys"));

            services.AddMemoryCache();

            services.AddHttpContextAccessor();

            services.AddControllers();

            services.AddMvcCore(config =>
            {
                config.EnableEndpointRouting = false;
            })
            .AddViews()
            .AddApiExplorer();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Open World Server Authentication API", Version = "v1" });

                c.AddSecurityDefinition("X-CustomerGUID", new OpenApiSecurityScheme
                {
                    Description = "Authorization header using the X-CustomerGUID scheme",
                    Name = "X-CustomerGUID",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "X-CustomerGUID"
                });

                c.OperationFilter<SwaggerSecurityRequirementsDocumentFilter>();

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "OWSPublicAPI.xml");
                c.IncludeXmlComments(filePath);
            });

            var apiPathOptions = new OWSShared.Options.APIPathOptions();
            Configuration.GetSection(OWSShared.Options.APIPathOptions.SectionName).Bind(apiPathOptions);

            services.AddHttpClient("OWSInstanceManagement", c =>
            {
                c.BaseAddress = new Uri(apiPathOptions.InternalInstanceManagementApiURL);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "OWSPublicAPI");
            });

            services.Configure<OWSShared.Options.PublicAPIOptions>(Configuration.GetSection(OWSShared.Options.PublicAPIOptions.SectionName));
            services.Configure<OWSShared.Options.APIPathOptions>(Configuration.GetSection(OWSShared.Options.APIPathOptions.SectionName));
            services.Configure<OWSShared.Options.StorageOptions>(Configuration.GetSection(OWSShared.Options.StorageOptions.SectionName));
            services.Configure<OWSShared.Options.OWSStorageConfig>(Configuration.GetSection(nameof(OWSShared.Options.OWSStorageConfig)));
            // Register And Validate External Login Provider Options
            // services.ConfigureAndValidate<EpicOnlineServicesOptions>(ExternalLoginProviderOptions.EpicOnlineServices, Configuration.GetSection($"{ExternalLoginProviderOptions.SectionName}:{ExternalLoginProviderOptions.EpicOnlineServices}"));
            //InstanceLauncherStartup.ConfigureInstanceLauncherServices(services);
            InitializeContainer(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStoreCustomerGuidMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            //app.UseAuthentication();

            //app.UseStaticFiles();
            app.UseRouting();

            app.UseMvc();
            app.UseSwagger(/*c =>
            {
                c.RouteTemplate =
                    "api-docs/{documentName}/swagger.json";
            }*/);
            app.UseSwaggerUI(c =>
            {
                //c.RoutePrefix = "api-docs";
                c.SwaggerEndpoint("./v1/swagger.json", "Open World Server Authentication API");
            });
        }

        private void InitializeContainer(IServiceCollection services)
        {
            services.AddSingleton<IHeaderCustomerGUID, HeaderCustomerGUID>();

            var OWSStorageConfig = Configuration.GetSection("OWSStorageConfig");
            if (OWSStorageConfig.Exists())
            {
                string dbBackend = OWSStorageConfig.GetValue<string>("OWSDBBackend");

                switch (dbBackend)
                {
                    case "postgres":
                        services.AddScoped<IInstanceManagementRepository, OWSData.Repositories.Implementations.Postgres.InstanceManagementRepository>();
                        services.AddTransient<ICharactersRepository, OWSData.Repositories.Implementations.Postgres.CharactersRepository>();
                        services.AddTransient<IUsersRepository, OWSData.Repositories.Implementations.Postgres.UsersRepository>();
                        break;
                    case "mysql":
                        services.AddScoped<IInstanceManagementRepository, OWSData.Repositories.Implementations.MySQL.InstanceManagementRepository>();
                        services.AddTransient<ICharactersRepository, OWSData.Repositories.Implementations.MySQL.CharactersRepository>();
                        services.AddTransient<IUsersRepository, OWSData.Repositories.Implementations.MySQL.UsersRepository>();
                        break;
                    default: // Default to MSSQL
                        services.AddScoped<IInstanceManagementRepository, OWSData.Repositories.Implementations.MSSQL.InstanceManagementRepository>();
                        services.AddTransient<ICharactersRepository, OWSData.Repositories.Implementations.MSSQL.CharactersRepository>();
                        services.AddTransient<IUsersRepository, OWSData.Repositories.Implementations.MSSQL.UsersRepository>();
                        break;
                }
            }

            services.AddSingleton<IPublicAPIInputValidation, DefaultPublicAPIInputValidation>();
            services.AddSingleton<ICustomCharacterDataSelector, DefaultCustomCharacterDataSelector>();
            services.AddSingleton<IGetReadOnlyPublicCharacterData, DefaultGetReadOnlyPublicCharacterData>();

            //InstanceLauncherStartup.InitializeContainer(services);
        }
    }
}
