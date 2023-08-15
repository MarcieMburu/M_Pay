using API.Models;
using System.Linq;

namespace API.Models
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User GetUserByUsername(string username)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Username == username);
        }
    }
}
