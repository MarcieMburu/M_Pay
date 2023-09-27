
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerMassTransit
{
    public class Worker : BackgroundService
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<Worker> _logger;

        public Worker(IBusControl busControl, ILogger<Worker> logger)
        {
            _busControl = busControl;
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started Rabbit Mq Server");
          return _busControl.StartAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);

            await _busControl.StopAsync(stoppingToken);
            _logger.LogInformation("Stopped Rabbit Mq Server");
        }
    }
}
