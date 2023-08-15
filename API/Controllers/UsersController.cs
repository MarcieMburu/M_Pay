using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;

using System.Linq;
using System.Threading.Tasks;


namespace API.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiVersion("1.0")]
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public UsersController(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        


        [Microsoft.AspNetCore.Mvc.HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<User>> GetUser()
        {
            string username = HttpContext.User.Identity.Name;
            var user = await _dbContext.Users
               .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
    }

}


