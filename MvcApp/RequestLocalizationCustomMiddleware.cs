using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
 
using Microsoft.AspNetCore.Http;

namespace MvcApp
{
    
    public class RequestLocalizationCustomMiddleware
    {
       RequestDelegate _next;
 
        public RequestLocalizationCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        } 

        public async Task InvokeAsync(HttpContext context)
        {
            CultureInfo.CurrentCulture = Session.Language.GetCulture();
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
 
            await _next(context);
        }
    }
}
