using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Data;

using WorkerService;


//builder.Services.AddDbContext<PaymentGatewayContext>(options =>
 //   options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentGatewayContext") ?? throw new InvalidOperationException("Connection string 'PaymentGatewayContext' not found.")));




IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services )=>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<PaymentGatewayContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PaymentGateway"));
        }, ServiceLifetime.Scoped);

        services.AddHostedService<Worker>();

    })
    .Build();

await host.RunAsync();
