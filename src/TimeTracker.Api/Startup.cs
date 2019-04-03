using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TimeTracker.Data;

namespace TimeTracker.Api
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton(configuration);
            
            if (!environment.IsTest())
            {
                services.ConfigureGoogleAuth(
                    configuration["Authentication:Google:ClientId"], 
                    configuration["Authentication:Google:ClientSecret"]);
            }

            services.AddSwaggerDocument(configure =>
            {
                configure.PostProcess = document =>
                {
                    document.Info.Version = "beta";
                    document.Info.Title = "Time Tracker API";
                };
            });

            var connection = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TimeTrackerDbContext>(options => { options.UseSqlServer(connection); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles(); // support wwwroot

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // add support to load node_modules libraries in dev mode
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
                    RequestPath = new PathString("/node_modules")
                });
            }

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseAuthentication();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());


            app.UseSwagger(settings => { });
            app.UseSwaggerUi3(settings => { });

            app.UseMvc();
        }
    }
}