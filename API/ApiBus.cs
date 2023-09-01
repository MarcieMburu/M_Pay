using MassTransit;

namespace API
{
    public class ApiBus : BackgroundService
    {
        
            private readonly IBusControl _busControl;
            private readonly ILogger<ApiBus> _logger;

            public ApiBus(IBusControl busControl, ILogger<ApiBus> logger)
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
