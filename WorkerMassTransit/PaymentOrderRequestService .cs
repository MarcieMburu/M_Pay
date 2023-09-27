using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkerMassTransit;
using WorkerService;

namespace WorkerMassTransit
{
    public class PaymentOrderRequestService
    {
        private readonly IBus _bus;
        private readonly IRequestClient<PaymentMessage> _requestClient;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentOrderRequestService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public PaymentOrderRequestService(IBus bus, HttpClient httpClient, ILogger<PaymentOrderRequestService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _bus = bus;
            _httpClient = httpClient;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        public async Task PublishMessage(ProcessedPaymentMessage processedPaymentMessage)
        {

            var paymentMessageResponse = new PaymentMessage { 
                Id = processedPaymentMessage.Id,
            };


            _logger.LogInformation("Payment Response Published");

            await _bus.Publish(paymentMessageResponse);

            await _publishEndpoint.Publish(new PaymentMessage { 

            Id= processedPaymentMessage.Id,

            });
            Console.WriteLine($"Published message:");

        }
    }
}
