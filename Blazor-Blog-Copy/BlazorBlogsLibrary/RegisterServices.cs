using System;
using System.Net.Http;
using BlazorBlogs.Data;
using BlazorBlogsLibrary.Areas.Identity;
using BlazorBlogsLibrary.Classes;
using BlazorBlogsLibrary.Data;
using BlazorBlogsLibrary.Data.Models;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using WilderMinds.MetaWeblog;
using MetaWeblogService = BlazorBlogsLibrary.Controllers.MetaWeblogService;

namespace BlazorBlogsLibrary
{
    public static class RegisterServices
    {
        public static IServiceCollection AddBlazorBlogsServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<BlazorBlogsContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(
                    options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddCircuitOptions(options => { options.DetailedErrors = true; });

            services.AddScoped<AuthenticationStateProvider,
                RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            // Allows appsettings.json to be updated programatically
            services.ConfigureWritable<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
            services.AddSingleton(configuration);

            services.AddScoped<BlogsService>();
            services.AddScoped<GeneralSettingsService>();
            services.AddScoped<EmailService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<SearchState>();
            services.AddScoped<DisqusState>();
            services.AddScoped<InstallUpdateState>();
            services.AddMetaWeblog<MetaWeblogService>();
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();
            services.AddScoped<HttpClient>();
            services.AddBlazoredToast();
            services.AddHeadElementHelper();

            return services;
        }
    }
}