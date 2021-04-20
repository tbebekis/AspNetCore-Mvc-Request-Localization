# Asp.Net Core 3.0 MVC Request Localization or how to set the Culture of a User Session

**Source code** can be found at [GitHub](https://github.com/tbebekis/AspNetCore-Mvc-Request-Localization).

## Introduction
A web site may provide a way for the visitor to select a preferred language for the displayed content. 

After such a selection is made, the web site has a number of options as to how that preferred language should be attached to visitor's session and its upcoming requests.

- It may use the route url to convey the information, e.g. `https://company.com/en/home/index`.
- It may use the query string, e.g. `https://company.com/home/index?lang=en`.
- It may store the information in a [Session Variable](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state#session-state)
- It may use cookies.
- Or it may even trust on the current [Accept-Language HTTP header](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language).

### Mapping a language to a Culture

Using the language information, as described above, the web site presents localized content to the visitor, by mapping a [Culture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo) to that language.

The first step in localizing an Asp.Net MVC application is to set the [Culture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo) of the [HTTP](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) [Request](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest). 

A **culture code** is something like `en-US`, which means `english-United States`, or `el-GR`, which means `greek-Greece`. The first part, as the `en` above, identifies the language, while the second part, as the `US` above, denotes a certain configuration used in handling dates, numbers and text issues.

Thus the culture used, while serving a request, has impact not only in choosing localized content or localized string resources but even how dates and numbers are formatted. Therefore it's a crucial issue.

## Background

A classic Asp.Net Framework MVC application uses the `MvcApplication` class, found in the `Global.asax.cs` file, which is a [HttpApplication](https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication) derived class. In the `Application_BeginRequest()` method of that `MvcApplication` class, the developer writes something like the following:

```
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // code here
        }

        CultureInfo GetRequestCultureFromSomeWhere()
        {
            return null;  // return a CultureInfo instance here
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            CultureInfo Culture = GetRequestCultureFromSomeWhere();

            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
        }
    }
```
The above sets the culture of the current thread, which is the thread that serves the request. That's all that needs to be done.

In the Asp.Net Core 3.0 things are ...improved. Let's explore the new options.

## Asp.Net Core 3.0 MVC Request Localization
There are two options:
- either use a classic approach, that is a  [`Middleware`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware) that handles the culture changing. You may find an example of that approach in [Microsoft's documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write#middleware-class). Another one example using Middleware is provided by the sample application of this post.
- or use a `Request Culture` Provider. 

### The Middleware option

This is easy. Just go to `ConfigureServices()` method of the `Startup` class, and just before the `app.UseEndpoints()` call or the `app.UseMvc()` call, code and *"inline"* middleware, something like the following

```
    app.Use(async (context, next) =>
    {
        CultureInfo.CurrentCulture = GetRequestCultureFromSomeWhere();
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

        await next.Invoke();
    })
```

Or if you prefer a stand-alone middleware class, code something like the following

```
    public class RequestLocalizationCustomMiddleware
    {
       RequestDelegate _next;
 
        public RequestLocalizationCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        } 

        public async Task InvokeAsync(HttpContext context)
        {
            CultureInfo.CurrentCulture = GetRequestCultureFromSomeWhere();
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
 
            await _next(context);
        }
    }
```
and then adjust the `ConfigureServices()` to use it as

```
app.UseMiddleware<RequestLocalizationCustomMiddleware>();
```

### The Request Culture Provider option

The first step is to configure the services properly.

In case your application needs just a single culture and uses that to handle all requests

```
    public void ConfigureServices(IServiceCollection services)
    {
        // code here

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-US");
        });

        services.AddMvc();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
         
        // code here
        
        app.UseRequestLocalization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

    }    
```

The `app.UseRequestLocalization()` call adds the  [RequestLocalizationMiddleware](https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/Localization/src/RequestLocalizationMiddleware.cs) which adds the three built-in Request Culture Providers:
- the [QueryStringRequestCultureProvider](https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/Localization/src/QueryStringRequestCultureProvider.cs)
- the [CookieRequestCultureProvider](https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/Localization/src/CookieRequestCultureProvider.cs)
- the [AcceptLanguageHeaderRequestCultureProvider](https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/Localization/src/AcceptLanguageHeaderRequestCultureProvider.cs)

You can remove those providers from the configuration, if you think you don't need them.

```
    public void ConfigureServices(IServiceCollection services)
    {
        // code here

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.RequestCultureProviders.Clear(); // <<<<
        });

        services.AddMvc();
    }
```

Or you may choose to just add a custom Request Culture Provider on top of the list, say a `CustomRequestCultureProvider`. Well, surprise, there is already a `CustomRequestCultureProvider` in Asp.Net Core 3.0 [source code](https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/Localization/src/CustomRequestCultureProvider.cs)

And here is how to use it

```
    public void ConfigureServices(IServiceCollection services)
    {
        // code here

        CustomRequestCultureProvider Provider = new CustomRequestCultureProvider(async (HttpContext) => {
            await Task.Yield();
            CultureInfo CI = GetRequestCultureFromSomeWhere();
            return new ProviderCultureResult(CI.Name);
        });

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture('en-US');
            options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US"), new CultureInfo("el-GR") };
            options.SupportedUICultures = options.SupportedCultures;

            //options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Insert(0, Provider);
        }); 

        services.AddMvc();
    }
```

> **NOTE**: `SupportedCultures` property defines a list of cultures supported by the application. For a culture to be used by the application as the current culture, it must be contained in the `SupportedCultures`, as seen above. Your `GetRequestCultureFromSomeWhere()` should return one of the `SupportedCultures`.

Of course you may use your own custom `Request Culture Provider` class. Something like the following

```
    public class MyCustomRequestCultureProvider : RequestCultureProvider
    {
        public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            await Task.Yield();
            CultureInfo CI = GetRequestCultureFromSomeWhere();
            return new ProviderCultureResult(CI.Name);
        }
    }
```

and register it as

```
    public void ConfigureServices(IServiceCollection services)
    {
        // code here

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture('en-US');
            options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US"), new CultureInfo("el-GR") };
            options.SupportedUICultures = options.SupportedCultures;

            //options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Insert(0, new MyCustomRequestCultureProvider());
        }); 

        services.AddMvc();
    }
```
 
## A sample application

There is an Asp.Net Core 3.0 MVC application that accompanies this post.

### The Startup class

The sample application provides a `Startup` class that uses both variations, `Middleware` or `Request Culture Provider`, based on a flag.

```
bool UseRequestLocalizationProvider = false;
```

Set it to `true` to use the new toys.

There is a predefined list of languages.

```
    void LoadLanguages()
    {
        var En = new LanguageItem() { Id = "", Name = "English", Code = "en", CultureCode = "en-US" };
        var Gr = new LanguageItem() { Id = "", Name = "Greek", Code = "el", CultureCode = "el-GR" };
        Languages.Add(En);
        Languages.Add(Gr);
    }
```

Both variations use the same custom static `Session` class where the sample application stores visitor's preferred language and any session information. Thus, `services.AddSession()` and `app.UseSession()` are called by the `Startup` class.


That `Session` class uses the `HttpContext.Session` property, in keeping that session information, so it needs an `IHttpContextAccessor` instance. Therefore the `Startup` class calls `services.AddHttpContextAccessor()`.

Also a `Session.HttpContextAccessor` property is assigned properly in the `Configure()` method.

```
Session.HttpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
```

### The LanguageSelector ViewComponent

The `LanguageSelector` class is a very basic [ViewComponent](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-components).

```
    public class LanguageSelector : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var List = Languages.Items;
            return View(List);
        }
    }
```

It renders html markup for setting the language. Here is the `Default.cshtml` of the `LanguageSelector` ViewComponent.

```
@model LanguageItem[]

@if (Model != null & Model.Length > 0)
{
    <div style="display: flex; flex-direction:row-reverse; padding-right: 4px">
        <ul style="list-style: none; display: flex">
            @foreach (var Item in Model)
            {
                <li style="padding:0 8px">
                    <a asp-controller="Home" asp-action="SetLanguage" asp-route-LanguageCode="@Item.Code">
                        @Item.Name
                    </a>
                </li>
            }
        </ul>
    </div>
}
```
It routes back to `HomeController`'s `SetLanguage()` action which handles the request.

```
    public IActionResult SetLanguage(string LanguageCode)
    {
        LanguageItem Lang = Languages.Find(LanguageCode);
        if (Lang != null && Lang.CultureCode != Session.Language.CultureCode)
        {
            Session.Language = Lang;
        }

        return RedirectToAction("Index");
    }
```

The `LanguageSelector` ViewComponent renders its markup at the far right of the navigation bar. For that the `_Layout.cshtml` contains the following.

```
@await Component.InvokeAsync("LanguageSelector")
```

## The lang attribute

The sample application writes the Culture code of the selected language in the `lang` [attribute](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/lang) of the `html` element.

```
<html lang="@Session.Language.CultureCode">
```

Thus becomes easy for a `javascript` code to get the selected Culture and language.

```
let CultureCode = document.querySelector('html').getAttribute('lang');
```

After that the script may use that culture in formatting numbers and dates.

```
let n = 123.456;
let S = n.toLocaleString(CultureCode);

let DT = new Date();
S = DT.toLocaleDateString(CultureCode);
S = DT.toLocaleTimeString(CultureCode);
```

## Conclusion
Setting the culture of a request in an Asp.Net Core 3.0 is a matter of setting the culture of the thread that is going to handle the request. There are two ways to achive that: either use a Middleware, a classic solution, or if you prefer the ...improved way, use a Request Culture Provider.




 




 