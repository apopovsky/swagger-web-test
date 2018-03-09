using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SwaggerWeb
{
    using System.IO;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Swashbuckle.AspNetCore.Swagger;

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
            services.AddMvc();

            services.AddSwaggerGen(o =>
            {
                var fileName = GetType().GetTypeInfo().Module.Name.Replace(".dll", ".xml").Replace(".exe", ".xml");
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, fileName));
                o.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            }).Configure<RouteOptions>(o =>
            {
                o.ConstraintMap.Add(SectorRouteConstraint.Key, typeof(SectorRouteConstraint));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseSwagger()
                .UseSwaggerUI(o =>
                {
                    o.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "V1");
                })
                .UseMvc();
        }
    }

    /// <summary>
    /// Sector constraint
    /// </summary>
    public class SectorRouteConstraint:IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            return true;
        }

        public static string Key => "sector";
    }
}
