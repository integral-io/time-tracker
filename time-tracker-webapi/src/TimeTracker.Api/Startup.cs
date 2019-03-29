using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;

namespace TimeTracker.Api
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton(Configuration);
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x => x.UseGoogle(
                    clientId: Configuration["Authentication:Google:ClientId"]));
            
            services.AddSwaggerDocument(configure => 
            { 
                configure.PostProcess = document =>
                {
                    document.Info.Version = "beta";
                    document.Info.Title = "Time Tracker API";
                    
                };
            });
            var connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TimeTrackerDbContext>(options =>
            {
                options.UseSqlServer(connection);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles(); // support wwwroot
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
                
                app.UseAuthentication();
            }

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
