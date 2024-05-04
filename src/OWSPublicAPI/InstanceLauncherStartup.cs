using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OWSData.Repositories.Interfaces;
using OWSShared.Extensions;
using Serilog;

namespace OWSPublicAPI
{
    public static class InstanceLauncherStartup
    {
        //Container container;
        private static OWSShared.Options.OWSInstanceLauncherOptions owsInstanceLauncherOptions;

        private static IConfiguration Configuration;

        public static void AddInstanceLauncherStartupOptions(IConfiguration configuration)
        {
            Configuration = configuration;

            owsInstanceLauncherOptions = new OWSShared.Options.OWSInstanceLauncherOptions();
            Configuration.GetSection(OWSShared.Options.OWSInstanceLauncherOptions.SectionName).Bind(owsInstanceLauncherOptions);

            //Check appsettings.json file for potential errors
            bool thereWasAStartupError = false;

            Log.Information("Checking appsettings.json for errors...");

            //Abort if there is not a valid OWSAPIKey in appsettings.json
            if (string.IsNullOrEmpty(owsInstanceLauncherOptions.OWSAPIKey))
            {
                thereWasAStartupError = true;
                Log.Error("Please enter a valid OWSAPIKey in appsettings.json!");
            }
            //Abort if there is not a valid PathToDedicatedServer in appsettings.json
            else if (string.IsNullOrEmpty(owsInstanceLauncherOptions.PathToDedicatedServer))
            {
                thereWasAStartupError = true;
                Log.Error("Please enter a valid PathToDedicatedServer in appsettings.json!");
            }
            //Check that a file exists at PathToDedicatedServer
            else if (!File.Exists(OperatingSystemExtension.PathCombine(owsInstanceLauncherOptions.PathToDedicatedServer)))
            {
                thereWasAStartupError = true;
                Log.Error("Your PathToDedicatedServer in appsettings.json points to a file that does not exist!  Please either point PathToDedicatedServer to your UE Editor exe or to your packaged UE dedicated server exe!");
            }
            //If using the UE4 editor, make sure there is a project path in Path To UProject
            else
            {
                if (OperatingSystem.IsWindows() && owsInstanceLauncherOptions.PathToDedicatedServer.Contains("Editor.exe") ||
                    OperatingSystem.IsMacOS() && owsInstanceLauncherOptions.PathToDedicatedServer.EndsWith("UnrealEditor") ||
                    OperatingSystem.IsLinux() && owsInstanceLauncherOptions.PathToDedicatedServer.EndsWith("Editor"))
                {
                    string serverArgumentsProjectPattern = @"^.*.uproject";
                    MatchCollection testForUprojectPath = Regex.Matches(owsInstanceLauncherOptions.PathToUProject, serverArgumentsProjectPattern);
                    if (testForUprojectPath.Count == 1)
                    {
                        Match testForUprojectPathMatch = testForUprojectPath.First();
                        string foundUprojectPath = testForUprojectPathMatch.Value;
                        if (!File.Exists(OperatingSystemExtension.PathCombine(foundUprojectPath)))
                        {
                            thereWasAStartupError = true;
                            Log.Error("Your PathToUProject in appsettings.json points to a uproject file that does not exist!");
                        }
                    }
                    else
                    {
                        thereWasAStartupError = true;
                        Log.Error("Because you are using UE4Editor.exe or UnrealEditor.exe, your PathToUProject in appsettings.json must contain a path to the uproject file.  Be sure to use escaped (double) backslashes in the path!");
                    }
                }
            }

            //If there was a startup error, don't continue any further.  Wait for shutdown.
            if (thereWasAStartupError)
            {
                Log.Fatal("Error encountered.  Shutting down...");
                Environment.Exit(-1);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureInstanceLauncherServices(IServiceCollection services)
        {

            services.ConfigureWritable<OWSShared.Options.OWSInstanceLauncherOptions>(Configuration.GetSection(OWSShared.Options.OWSInstanceLauncherOptions.SectionName));
            services.Configure<OWSShared.Options.APIPathOptions>(Configuration.GetSection(OWSShared.Options.APIPathOptions.SectionName));

            var apiPathOptions = new OWSShared.Options.APIPathOptions();
            Configuration.GetSection(OWSShared.Options.APIPathOptions.SectionName).Bind(apiPathOptions);

            InitializeContainer(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        //{

        //    app.UseHttpsRedirection();
        //    app.UseStaticFiles();

        //    app.UseRouting();

        //    app.UseAuthorization();

        //    app.UseEndpoints(endpoints =>
        //    {
        //        endpoints.MapRazorPages();
        //    });
        //}

        public static void InitializeContainer(IServiceCollection services)
        {
            //Register our ZoneServerProcessesRepository to store a list of our running zone server processes for this hardware device
            //container.Register<IZoneServerProcessesRepository, OWSData.Repositories.Implementations.InMemory.ZoneServerProcessesRepository>(Lifestyle.Singleton);
            services.AddSingleton<IZoneServerProcessesRepository, OWSData.Repositories.Implementations.InMemory.ZoneServerProcessesRepository>();

            //Register our OWSInstanceLauncherDataRepository to store information that needs to be shared amongst the jobs
            //container.Register<IOWSInstanceLauncherDataRepository, OWSData.Repositories.Implementations.InMemory.OWSInstanceLauncherDataRepository> (Lifestyle.Singleton);
            services.AddSingleton<IOWSInstanceLauncherDataRepository, OWSData.Repositories.Implementations.InMemory.OWSInstanceLauncherDataRepository>();

            //ServerLauncherMQListener runs only once
            //container.RegisterInstance(new TimedHostedService<IInstanceLauncherJob>.Settings(
            //    interval: TimeSpan.FromSeconds(10),
            //    runOnce: true,
            //    action: processor => processor.DoWork(),
            //    dispose: processor => processor.Dispose()
            //));

            //ServerLauncherHealthMonitoring runs once every X seconds.  X is configured in the OWSInstanceLauncherOptions in appsettings.json
            //services.AddHostedService<IServerHealthMonitoringJob>(); todo rewrite as a background service
            //container.RegisterInstance(new TimedHostedService<IServerHealthMonitoringJob>.Settings(
            //    interval: TimeSpan.FromSeconds(owsInstanceLauncherOptions.RunServerHealthMonitoringFrequencyInSeconds),
            //    runOnce: false,
            //    action: processor => processor.DoWork(),
            //    dispose: processor => processor.Dispose()
            //));

            //Register our Server Launcher MQ Listener job
            //container.RegisterSingleton<IInstanceLauncherJob, ServerLauncherMQListener>();

            //Register our Server Launcher Health Monitoring Job
            //container.RegisterSingleton<IServerHealthMonitoringJob, ServerLauncherHealthMonitoring>();
        }

    }
}