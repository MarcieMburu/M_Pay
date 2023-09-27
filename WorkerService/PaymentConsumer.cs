using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WorkerService;

namespace WorkerService
{
    public class PaymentConsumer : IConsumer<TransactionViewModel>
    {
        public ILogger<PaymentConsumer> _logger;
        private readonly HttpClient _httpClient;
        public PaymentConsumer(ILogger<PaymentConsumer> logger, HttpClient httpClient)


        {
            _logger = logger;
            _httpClient = httpClient;

        }





        async Task IConsumer<TransactionViewModel>.Consume(ConsumeContext<TransactionViewModel> context)
        {

          
            _logger.LogInformation("Payment Response Received By Consumer for Transaction {Id}", context.Message.Id);


            var paymentResponseMessage = context.Message;



            var receivedResponse = new Proccessess
            {

                Id = paymentResponseMessage.Id,
                Amount = paymentResponseMessage.Amount,
                SenderName = paymentResponseMessage.SenderName



            };
            try
            {
                await context.RespondAsync(receivedResponse);


                await context.Publish(receivedResponse);

                _logger.LogInformation($"Transaction with  ID  {paymentResponseMessage.Id} successfully processed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing transaction {ex.Message}");
            }
            // await context.RespondAsync(paymentMessageResponse);


        }
    }

}
