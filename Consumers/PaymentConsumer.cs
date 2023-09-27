using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentGateway.Models;


namespace WorkerService
{
    public class PaymentConsumer : IConsumer<TransactionViewModel>
    {
        public ILogger<PaymentConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        public PaymentConsumer(ILogger<PaymentConsumer> logger,   IServiceProvider serviceProvider)


        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }





        public  async Task Consume(ConsumeContext<TransactionViewModel> context)
        {


            _logger.LogInformation("Payment Response Received By Consumer for Transaction {Id}", context.Message.Id);


            var paymentResponseMessage = context.Message;



            var receivedResponse = new ProcessedTransactionViewModel
            {

                Id = paymentResponseMessage.Id,
                Amount = paymentResponseMessage.Amount,
                SenderName = paymentResponseMessage.SenderName



            };
            try
            {
                await context.RespondAsync(receivedResponse);



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
