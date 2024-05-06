﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OWSData.Repositories.Interfaces;
using OWSShared.Options;
using OWS.Silo;
using OWSShared.Implementations;
using OWSShared.Interfaces;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

InstanceLauncherStartup.AddInstanceLauncherStartupOptions(configuration);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddConfiguration(configuration);

OWSStorageConfig storageOptions = new();
builder.Configuration.GetSection(nameof(OWSStorageConfig)).Bind(storageOptions);

builder.Services.Configure<OWSStorageConfig>(
    builder.Configuration.GetSection(
        key: nameof(OWSStorageConfig)));

builder.Services.Configure<OWSShared.Options.RabbitMQOptions>(builder.Configuration.GetSection(RabbitMQOptions.SectionName));

builder.UseOrleans(silo =>
{
    silo.UseInMemoryReminderService();
    
    silo.UseLocalhostClustering();
    silo.AddAdoNetGrainStorage("OrleansStorage", options =>
    {
        options.Invariant = "Microsoft.Data.SqlClient";
        options.ConnectionString = storageOptions.OWSDBConnectionString;
    });
    silo.UseDashboard();
});

switch (storageOptions.OWSDBBackend)
{
    case "postgres":
        builder.Services.AddScoped<IInstanceManagementRepository, OWSData.Repositories.Implementations.Postgres.InstanceManagementRepository>();
        builder.Services.AddTransient<ICharactersRepository, OWSData.Repositories.Implementations.Postgres.CharactersRepository>();
        builder.Services.AddTransient<IUsersRepository, OWSData.Repositories.Implementations.Postgres.UsersRepository>();
        break;
    case "mysql":
        builder.Services.AddScoped<IInstanceManagementRepository, OWSData.Repositories.Implementations.MySQL.InstanceManagementRepository>();
        builder.Services.AddTransient<ICharactersRepository, OWSData.Repositories.Implementations.MySQL.CharactersRepository>();
        builder.Services.AddTransient<IUsersRepository, OWSData.Repositories.Implementations.MySQL.UsersRepository>();
        break;
    default: // Default to MSSQL
        builder.Services.AddScoped<IInstanceManagementRepository, OWSData.Repositories.Implementations.MSSQL.InstanceManagementRepository>();
        builder.Services.AddTransient<ICharactersRepository, OWSData.Repositories.Implementations.MSSQL.CharactersRepository>();
        builder.Services.AddTransient<IUsersRepository, OWSData.Repositories.Implementations.MSSQL.UsersRepository>();
        break;
}


builder.Services.AddLogging();
builder.Logging.AddConsole();

InstanceLauncherStartup.ConfigureInstanceLauncherServices(builder.Services);
builder.Services.AddSingleton<IZoneServerProcessesRepository, OWSData.Repositories.Implementations.InMemory.ZoneServerProcessesRepository>();
builder.Services.AddSingleton<IOWSInstanceLauncherDataRepository, OWSData.Repositories.Implementations.InMemory.OWSInstanceLauncherDataRepository>();
builder.Services.AddSingleton<ICustomCharacterDataSelector, DefaultCustomCharacterDataSelector>();
builder.Services.AddSingleton<IGetReadOnlyPublicCharacterData, DefaultGetReadOnlyPublicCharacterData>();

using IHost host = builder.Build();


await host.RunAsync();