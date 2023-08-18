using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using API.Models;
using PaymentGateway.Data;

namespace API.Attributes
{
    public class UserService : IUserService
    {
        private readonly PaymentGatewayContext _context;
        private readonly IConfiguration _configuration;


        public UserService(PaymentGatewayContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
          //  var user = await _context.Transaction.FirstOrDefaultAsync(x => x.Username == username);

          //  if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

         //   return user;
        }
        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return false;
                }
            }
            return true;
        }


    }
}
