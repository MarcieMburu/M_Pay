//using MassTransit;

//namespace API
//{
//    public class MessageService
//    {
//        private readonly IBus _bus;
//        private readonly IRequestClient<TransactionProcessedResponse> _requestClient;
//        private readonly HttpClient _httpClient;
//        private readonly ILogger<MessageService> _logger;


//        public MessageService(IBus bus, HttpClient httpClient, ILogger<MessageService> logger)
//        {
//            _bus = bus;
//            _httpClient = httpClient;
//            _logger = logger;
//        }

//        public async Task SendMessageAsync(string messageContent)
//        {
//            await _bus.Publish<TransactionProcessedResponse>(new { Content = messageContent });
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await _bus.Publish<TransactionProcessedResponse>(new TransactionProcessedResponse
//                {
//                    MessageResponse = "hello"
//                },
//                stoppingToken);
//                await Task.Delay(1000, stoppingToken);

//                        }
//            }
//    }
//}
