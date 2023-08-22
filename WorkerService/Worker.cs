using PaymentGateway.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PaymentGatewayContext _context;
        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, PaymentGatewayContext context)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Transaction status update worker is running...");

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var Context = scope.ServiceProvider.GetRequiredService<PaymentGatewayContext>();

                    // Query the Zamupay API for transaction statuses
                    // Update your database with the retrieved statuses

                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); // Delay before next iteration
                }
            }
        }
    }
}