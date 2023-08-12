using System.Collections.Generic;

namespace API.Models
{
    public class UserRepository : IUserRepository
    {
        public bool CheckUser(string username, string password)
        {
            return username.Equals("Mercy") && password.Equals("212");
        }

        public bool IsValid(string authHeader)
        {
            throw new NotImplementedException();
        }
        //private List<User> _users = new List<User>
        //{
        //    new User
        //    {
        //        Id = 1, Username = "peter", Password = "peter123"
        //    },
        //    new User
        //    {
        //        Id = 2, Username = "joydip", Password = "joydip123"
        //    },
        //    new User
        //    {
        //        Id = 3, Username = "james", Password = "james123"
        //    }
        //};
        //public async Task<bool> Authenticate(string username, string password)
        //{
        //    if (await Task.FromResult(_users.SingleOrDefault(x => x.Username == username && x.Password == password)) != null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //public async Task<List<string>> GetUserNames()
        //{
        //    List<string> users = new List<string>();
        //    foreach (var user in _users)
        //    {
        //        users.Add(user.Username);
        //    }
        //    return await Task.FromResult(users);
        //}

        //Task IUserRepository.Authenticate(string username, string password)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
