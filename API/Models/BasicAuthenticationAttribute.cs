//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System;
//using System.Net;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;


//namespace API.Models
//{
//    public class BasicAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
//    {
//        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
//        {
//            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
//            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

//            if (!await authService.IsValidAsync(authHeader))
//            {
//                context.Result = new UnauthorizedResult();
//            }
//        }
//    }

//    public interface IAuthService
//    {
//        Task<bool> IsValidAsync(string authHeader);
//    }

//    public class AuthService : IAuthService
//    {
//        private readonly IUserRepository _userRepository;

//        public AuthService(IUserRepository userRepository)
//        {
//            _userRepository = userRepository;
//        }

//        public async Task<bool> IsValidAsync(string authHeader)
//        {
//            if (!AuthenticationHeaderValue.TryParse(authHeader, out var headerValue) ||
//                headerValue.Scheme != "Basic")
//            {
//                return false;
//            }

//            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue.Parameter));
//            var usernamePassword = credentials.Split(':', 2);
//            var username = usernamePassword[0];
//            var password = usernamePassword[1];

//            var user = await _userRepository.GetUserByUsername(username);

//            if (user == null || user.Password != password)
//            {
//                return false;
//            }

//            return true;
//        }
//    }

//    public interface IUserRepository
//    {
//        Task<User> GetUserByUsername(string username);
//    }

//    public class UserService : IUserRepository
//    {
//        private readonly List<User> _users = new List<User>
//        {
//            new User { Id = 1, Username = "user1", Password = "password1" },
//            new User { Id = 2, Username = "user2", Password = "password2" }
//            // Add more users as needed
//        };

//        public Task<User> GetUserByUsername(string username)
//        {
//            return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
//        }
//    }
//}