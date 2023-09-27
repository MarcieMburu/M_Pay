using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WorkerMassTransit;
using WorkerService;

namespace WorkerMassTransit
{
    public class PaymentConsumer : IConsumer<PaymentMessage>
    {
        public ILogger<PaymentConsumer> _logger;
        private readonly HttpClient _httpClient;
        public PaymentConsumer(ILogger<PaymentConsumer> logger,HttpClient httpClient)


        {
            _logger = logger;
            _httpClient = httpClient;
          
        }


 


         async Task IConsumer<PaymentMessage>.Consume(ConsumeContext<PaymentMessage> context)
        {

            //var paymentMessageResponse = new PaymentMessage
            //{
            //    Id = context.Message.Id   
                
                
            //};

            _logger.LogInformation("Payment Response Received By Consumer");


            var paymentResponseMessage = context.Message;



            var receivedResponse = new ProcessedPaymentMessage {

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
