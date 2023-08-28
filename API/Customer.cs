//using MassTransit;
//namespace API
//{
//    public class Customer : IConsumer<TransactionProcessedResponse>
//    {
//        public ILogger<Customer> _logger;
//        public Customer(ILogger<Customer> logger)

//        { _logger = logger; }
     

//        async Task IConsumer<TransactionProcessedResponse>.Consume(ConsumeContext<TransactionProcessedResponse> context)
//        {
//            _logger.LogInformation("Received Message {Content}", context.Message.MessageResponse);
//            var response = new TransactionProcessedResponse
//            {
//               IsQueued = true,
//                MessageResponse = "Transaction processed successfully"
//            };

//            await context.RespondAsync(response);
//            //return Task.CompletedTask;

//        }
//    }

//    public class TransactionProcessedResponse
//    {
//        public bool IsQueued { get; set; }
//        public string MessageResponse { get; init; }
//    }
//}