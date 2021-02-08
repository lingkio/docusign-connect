using System.Threading.Tasks;
using Docusign_Connect.Constants;
using Microsoft.AspNetCore.Http;
namespace Docusign_Connect.Middleware
{
    public class RedirecterMiddleware
    {
        private readonly RequestDelegate _next;

        //Your constructor will have the dependencies needed for database access
        public RedirecterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToUriComponent();
            context.Session.SetString(LingkConst.SessionKey, path);
            await _next(context);
        }
    }
}