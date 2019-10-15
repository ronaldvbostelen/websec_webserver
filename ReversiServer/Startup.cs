using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using reCAPTCHA.AspNetCore;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;

namespace ReversiServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddFile(AppContext.BaseDirectory + "app.log", append: true);
            });
            
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.ConsentCookie.Name = "CookieConsent";
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(opt =>
            {
                opt.Cookie.SameSite = SameSiteMode.Strict;
                opt.Cookie.Name = "id";
                opt.IdleTimeout = TimeSpan.FromMinutes(30);
                opt.Cookie.HttpOnly = true;
                opt.Cookie.IsEssential = true;
            });

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Configuration.GetSection("AuthCookiePath").Value))
                .SetApplicationName("Reversi");

            services.AddAuthentication("Identity.Application")
                .AddCookie("Identity.Application", opts =>
                {
                    opts.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    opts.SlidingExpiration = true;
                    opts.AccessDeniedPath = new PathString("/Forbidden");
                    opts.LoginPath = new PathString("/Index");
                    opts.Cookie.Name = "whoami";
                });
                


            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "AntiForgeryToken";
            });

            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));
            services.AddScoped<IRecaptchaService, RecaptchaService>();
            services.AddScoped<IReversiApi, ReversiApi>();
            services.AddScoped<IUserIdentity, UserIdentity>();
            services.AddScoped<IPasswordValidator, StringValidator>();
            services.AddScoped<IJArrayToClaimsList, ArrayToClaimsList>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddSessionStateTempDataProvider();

            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseMvc();
        }
    }
}