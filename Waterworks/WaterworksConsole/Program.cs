using Application.Parley.Workflow.Nodes.CatFacts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parley.Configuration;
using WaterworksConsole.Application.Core.ConsoleMenu;
using WaterworksConsole.Application.Services;

var builder = Host.CreateApplicationBuilder(args);

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).AddUserSecrets<Program>().Build();

RegisterServices(builder.Services, configuration);

var host = builder.Build();

await ParleyConfiguration.PreloadNodes(host, typeof(CatFactsNode).Assembly);

var application = host.Services.GetService<ConsoleMenu>();

if (application != null)
    await application.Start();

static void RegisterServices(IServiceCollection services, IConfiguration configuration)
{
    ParleyConfiguration.ConfigureParley(services, configuration, true);

    services.AddScoped<IWorkflowService, WorkflowService>()
            .AddScoped<ConsoleMenu>();
}