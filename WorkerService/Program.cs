using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Data;
using PaymentGateway.Helpers;
using PaymentGateway.Models;
using WorkerService;





IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services )=>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<PaymentGatewayContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PaymentGatewayContext"));
        }, ServiceLifetime.Scoped);

        services.AddHostedService<Worker>();
        services.AddHttpClient();
        services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();
        services.AddAutoMapper(typeof(Program).Assembly);


    })
    .Build();

await host.RunAsync();
