using System.Threading.Tasks;
using Lingk_SAML_Example.Common;
using Microsoft.AspNetCore.Http;
namespace Lingk_SAML_Example.Middleware
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