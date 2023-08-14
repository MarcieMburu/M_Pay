using System.Text.Json.Serialization;

namespace PaymentGateway.DTOs
{
    public class AccessToken 
    {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
      
    }
}
