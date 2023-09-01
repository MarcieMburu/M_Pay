//using MassTransit;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using PaymentGateway.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;


//namespace API
//{
//    public class ApiPaymentConsumer : IConsumer<TransactionViewModel>
//    {
//        public ILogger<ApiPaymentConsumer> _logger;
//        private readonly HttpClient _httpClient;
//        public ApiPaymentConsumer(ILogger<ApiPaymentConsumer> logger, HttpClient httpClient)


//        {
//            _logger = logger;
//            _httpClient = httpClient;

//        }





//       public async Task Consume(ConsumeContext<TransactionViewModel> context)
//        {


//            _logger.LogInformation("Payment Response Received By Consumer");

//            try { 

//            var paymentResponseMessage = context.Message;



//            var receivedResponse = new ProcessedTransactionViewModel
//            {

//                Id = paymentResponseMessage.Id,
//                Amount = paymentResponseMessage.Amount,
//                SenderName = paymentResponseMessage.SenderName



//            };
           
//                await context.RespondAsync(receivedResponse);



//                _logger.LogInformation($"Transaction with  ID  {paymentResponseMessage.Id} successfully processed.");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error processing transaction {ex.Message}");
//            }
//            // await context.RespondAsync(paymentMessageResponse);


//        }
//    }

//    internal class ProcessedTransactionViewModel
//    {
//        public int Id { get; set; }
//        public int Amount { get; set; }
//        public string SenderName { get; set; }
//    }
//}
