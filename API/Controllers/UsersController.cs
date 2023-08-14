using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;


namespace API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [System.Web.Http.Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        List<User> _users = new List<User>();

        public UsersController()
        {
            for (int i = 1; i <= 9; i++)
            {
                _users.Add(new User()
                {
                    Id = i,
                    Username = "Mercy" + i,
                    Password = "Mercy1" + i,

                });
            }
        }


        // GET: api/<UsersController>
        [System.Web.Http.HttpGet]
        public IEnumerable<User> Get()
        {
            return _users;
        }

        //// GET api/<UsersController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<UsersController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<UsersController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<UsersController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}

