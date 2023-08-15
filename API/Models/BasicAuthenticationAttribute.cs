using System;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
{
    public override void OnAuthorization(HttpActionContext actionContext)
    {
        // Check if the Authorization header is present
        if (actionContext.Request.Headers.Authorization != null)
        {
            // Get the credentials from the header
            string authHeader = actionContext.Request.Headers.Authorization.Parameter;
            byte[] authBytes = Convert.FromBase64String(authHeader);
            string credentials = Encoding.UTF8.GetString(authBytes).Trim();

            // Split credentials into username and password
            string[] splitCredentials = credentials.Split(':');
            string username = splitCredentials[0];
            string password = splitCredentials[1];

            // Perform authentication logic (e.g., check against database)
            if (IsValidUser(username, password))
            {
                // User is authenticated, continue with the request
                return;
            }
        }

        // User is not authenticated, return Unauthorized response
        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
    }

    private bool IsValidUser(string username, string password)
    {
        // Implement your authentication logic here
        // You might check against a database or other authentication mechanism
        // Return true if the user is valid, false otherwise
        // Example:
        return username == "validuser" && password == "password123";
    }
}
