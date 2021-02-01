using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        context.Session.SetString("Templatepath", path);
        await _next(context);
    }
}