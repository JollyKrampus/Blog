using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BlazorBlogs
{
    //todo: link somewhere my GitHub, facebook, twitter, youtube, steam and xbox profiles
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}