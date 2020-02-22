using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace MvcApp
{
    public class MyCustomRequestCultureProvider : RequestCultureProvider
    {
		public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
            await Task.Yield();
            return new ProviderCultureResult(Session.Language.CultureCode);
        }
	}
}
