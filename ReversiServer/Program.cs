using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReversiServer
{
    public class Program
    {
        static string webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:5050")
                .ConfigureAppConfiguration((context, builder) => builder.SetBasePath(context.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", false,true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .Build())
                .UseWebRoot("wwwroot")
                .UseKestrel()
                .UseStartup<Startup>();
    }
}
