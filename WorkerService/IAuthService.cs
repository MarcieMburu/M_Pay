using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService
{
    public interface IAuthService
    {
        Task<string> GetBearerToken(string client_id, string client_secret);

    }
}
