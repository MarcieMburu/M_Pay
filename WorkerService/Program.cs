using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Helpers;
using PaymentGateway.Models;
using WorkerService;
using IHost = Microsoft.Extensions.Hosting.IHost;



IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddDbContext<PaymentGatewayContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PaymentGatewayContext"));
        }, ServiceLifetime.Scoped);

        services.AddHostedService<PaymentOrderRequest>();
       // services.AddHostedService<GetTransactionStatus>();
        services.AddDistributedMemoryCache();
        services.AddHttpClient();
        //services.AddScoped<IAuthService, AuthService>();

        //services.AddScoped<IRequestClient<TransactionViewModel>, RequestClient<TransactionViewModel>>();
        //(provider => (RequestClient<TransactionViewModel>)provider.GetRequiredService<IBusControl>().CreateRequestClient<TransactionViewModel>());


        //services.AddScoped<IRequestClient<ProcessedPaymentMessage>, RequestClient<ProcessedPaymentMessage>>(provider =>
        //   (RequestClient<ProcessedPaymentMessage>)provider.GetRequiredService<IBus>().CreateRequestClient<ProcessedPaymentMessage>());


        services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();
        services.AddAutoMapper(typeof(Program).Assembly);
        services.Configure<AuthService.ApiSettings>(configuration.GetSection("ApiSettings"));
        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = "localhost:6379";
        //    options.InstanceName = "MyRedis";
        //});
        services.AddMassTransit(configurator =>
        {

            configurator.UsingRabbitMq((context, cfg) =>
            {

                cfg.Host("localhost", "/", hostConfigurator =>
                {
                    hostConfigurator.Username("guest");
                    hostConfigurator.Password("guest");
                });
               
                cfg.ConfigureEndpoints(context);

                

            });
            //configurator.AddRequestClient<TransactionViewModel>();
            // configurator.AddRequestClient<ProcessedTransactionViewModel>();
            configurator.AddConsumersFromNamespaceContaining<PaymentConsumer>();

        });
       })
    .Build();

await host.RunAsync();
