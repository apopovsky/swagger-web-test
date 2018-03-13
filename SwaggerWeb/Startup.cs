using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SwaggerWeb
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.XPath;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Routing;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

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
                o.OperationFilter<OpFilter>(Path.Combine(AppContext.BaseDirectory, fileName));
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

    public class OpFilter:IOperationFilter
    {
        private readonly XPathNavigator _xmlNavigator;
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryXPath = "summary";
     
            
        public OpFilter(string filePath)
        {
            XPathDocument xmlDoc = new XPathDocument(filePath);
            _xmlNavigator = xmlDoc.CreateNavigator();
        }


        public void Apply(Operation operation, OperationFilterContext context)
        {
            ApplyConstraintsXmlToActionParameters(operation.Parameters, context.ApiDescription);
        }

        private void ApplyConstraintsXmlToActionParameters(IList<IParameter> parameters, ApiDescription apiDescription)
        {
            var nonBodyParameters = parameters.OfType<NonBodyParameter>();
            foreach (var parameter in nonBodyParameters)
            {                // Check for a corresponding action parameter?
                var actionParameter = apiDescription.ParameterDescriptions.FirstOrDefault(p =>
                    parameter.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (actionParameter == null) continue;

                if (!actionParameter.RouteInfo.Constraints.Any()) continue;

                var constraintType = actionParameter.RouteInfo.Constraints.FirstOrDefault().GetType();
                var commentIdForType = XmlCommentsIdHelper.GetCommentIdForType(constraintType);
                var constraintSummaryNode = _xmlNavigator
                    .SelectSingleNode(string.Format(MemberXPath, commentIdForType))
                    ?.SelectSingleNode(SummaryXPath);
                if (constraintSummaryNode != null)
                {
                    parameter.Description = XmlCommentsTextHelper.Humanize(constraintSummaryNode.InnerXml);
                }
            }
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
