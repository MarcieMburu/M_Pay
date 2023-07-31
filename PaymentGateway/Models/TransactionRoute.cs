namespace PaymentGateway.Models
{
    public class ChannelType
    {
        public int channelType { get; set; }
        public string channelDescription { get; set; }
        public string Value { get; internal set; }
        public string Text { get; internal set; }

        internal void Add(ChannelType channelType)
        {
            throw new NotImplementedException();
        }
    }

    public class TransactionRoute
    {
        public List<Route> routes { get; set; }
    }

    public class Route
    {
        public string id { get; set; }
        public string category { get; set; }
        public string categoryDescription { get; set; }
        public string transactionTypeId { get; set; }
        public bool categoryIsEnabled { get; set; }
        public string routeIntergration { get; set; }
        public string country { get; set; }
        public bool routeIsActive { get; set; }
        public List<ChannelType> channelTypes { get; set; }
    }
}