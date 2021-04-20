using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;


namespace MvcApp
{
    public class Startup
    {
        bool UseRequestLocalizationProvider = false;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        void LoadLanguages()
        {
            var En = new LanguageItem() { Id = "", Name = "English", Code = "en", CultureCode = "en-US" };
            var Gr = new LanguageItem() { Id = "", Name = "Greek", Code = "el", CultureCode = "el-GR" };
            Languages.Add(En);
            Languages.Add(Gr);
        }
        void ConfigureRequestLocalizationProvider(IServiceCollection services)
        {
            LanguageItem[] LangItems = Languages.Items;

            CustomRequestCultureProvider Provider = new CustomRequestCultureProvider(async (HttpContext) => {
                await Task.Yield();
                return new ProviderCultureResult(Session.Language.CultureCode);
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(LangItems[0].CultureCode);
                options.SupportedCultures = LangItems.Select(item => item.GetCulture()).ToList();
                options.SupportedUICultures = options.SupportedCultures;

                //options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Insert(0, Provider);
            });

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            LoadLanguages();

            if (UseRequestLocalizationProvider)
            {
                ConfigureRequestLocalizationProvider(services);
            }                

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddHttpContextAccessor();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Session.HttpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
 
            app.UseAuthorization();

            if (UseRequestLocalizationProvider)
            {
                // adds the Microsoft.AspNetCore.Localization.RequestLocalizationMiddleware to the pipeline
                app.UseRequestLocalization();
            }
            else
            {
                //app.UseMiddleware<RequestLocalizationCustomMiddleware>();

                app.Use(async (context, next) =>
                {
                    CultureInfo.CurrentCulture = Session.Language.GetCulture();
                    CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

                    await next.Invoke();
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }


}
