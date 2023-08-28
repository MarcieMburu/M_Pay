using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mass_Transit
{
    internal class MessageService 
    {
        private readonly IBus _bus;
        private readonly IRequestClient<TransactionProcessedResponse> _requestClient;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MessageService> _logger;
        public MessageService(IBus bus, HttpClient httpClient, ILogger<MessageService> logger)
        {
            _bus = bus;
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task SendMessageAsync(string messageContent)
        {
            await _bus.Publish(new TransactionProcessedResponse { MessageResponse = messageContent });
        }
    }
}
