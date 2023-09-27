using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace WorkerMassTransit
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
         await CreateHostBuilder(args).Build().RunAsync();
       }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(configurator =>
                    {
                        configurator.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host("localhost", "/",hostConfigurator =>
                            {
                                hostConfigurator.Username("guest");
                                hostConfigurator.Password("guest");
                            });


                            cfg.ConfigureEndpoints(context);
                        });

                        configurator.AddConsumersFromNamespaceContaining<PaymentConsumer>();
                    });


                    services.AddHttpClient();
                    services.AddHostedService<Worker>();
                });
        }
    }
}
