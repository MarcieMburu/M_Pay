using API.Models;

namespace API.Attributes
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
    }
}

