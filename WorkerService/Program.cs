using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Data;
using PaymentGateway.Helpers;
using PaymentGateway.Models;
using WorkerService;
using Microsoft.Extensions.Configuration;
using static WorkerService.GetTransactionStatus;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services )=>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<PaymentGatewayContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PaymentGatewayContext"));
        }, ServiceLifetime.Scoped);

        services.AddHostedService<GetTransactionStatus>();

        services.AddHostedService<PaymentOrderRequest>();
       
      

        services.AddHttpClient();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();
        services.AddAutoMapper(typeof(Program).Assembly);
        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = "localhost:6379";
        //    options.InstanceName = "MyRedis";
        //});

    })
    .Build();

await host.RunAsync();
