using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGateway.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService
{
    
    public class AuthService : IAuthService
    {
        private readonly ILogger<GetTransactionStatus> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly HttpClient _httpClient;
        private AccessToken _accessToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private ApiSettings _apiSettings;


        public AuthService(ILogger<GetTransactionStatus> logger, IServiceScopeFactory serviceScopeFactory,
            HttpClient httpClient, IHttpClientFactory httpClientFactory, IMapper mapper, IOptions<ApiSettings> apiSettings,
            IServiceProvider serviceProvider)
        {



            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _serviceProvider = serviceProvider;

        }
        public async Task<string> GetBearerToken(string client_id, string client_secret)
        {
            var requestContent = new FormUrlEncodedContent(new[]
                       {
                new KeyValuePair<string, string>("client_id",_apiSettings.client_id),
                new KeyValuePair<string, string>("client_secret",_apiSettings.client_secret),
                 new KeyValuePair<string, string>("grant_type",_apiSettings.grant_type),
                 new KeyValuePair<string, string>("scope",_apiSettings.scope)
            });

            try
            {
                var response = await _httpClient.PostAsync($"{_apiSettings.base_token_url}/connect/token", requestContent);
                //response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var accessToken = JsonConvert.DeserializeObject<AccessToken>(responseContent);


                return accessToken.access_token;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

       

        public class ApiSettings
        {
            public string client_id { get; set; }

            public string client_secret { get; set; }
            public string base_api_url { get; set; }
            public string base_token_url { get; set; }
            public string grant_type { get; set; }

            public string scope { get; set; }

        }
    }
}
