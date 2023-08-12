using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;

namespace API.Models
{
    public interface IUserRepository
    {
        bool CheckUser(string username, string password);
        bool IsValid(string authHeader);
    }
}
