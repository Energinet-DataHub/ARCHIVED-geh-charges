// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq;
using System.Text.Json.Serialization;
using Energinet.DataHub.Core.App.Common.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using GreenEnergyHub.Charges.WebApi.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace GreenEnergyHub.Charges.WebApi
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
            services.AddApplicationInsightsTelemetry();

            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .AddOData(option => option.Select().Filter().Count().OrderBy().Expand().SetMaxTop(100));

            services.AddSwaggerGen(c =>
            {
                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer", },
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement { { securitySchema, new[] { "Bearer" } }, };

                c.AddSecurityRequirement(securityRequirement);
            });

            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            // Make Swagger understand how to differentiate between different versions of endpoints based on decoration.
            // See https://referbruv.com/blog/posts/integrating-aspnet-core-api-versions-with-swagger-ui.
            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            services.ConfigureOptions<ConfigureSwaggerOptions>();
            services.AddQueryApi(Configuration);
            services.AddJwtTokenSecurity();

            // Health check
            services.AddHealthChecks()
                .AddLiveCheck()
                .AddSqlServer(
                    name: "ChargeDb",
                    connectionString: Configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var groupName in provider.ApiVersionDescriptions.Select(x => x.GroupName))
                    {
                        options.SwaggerEndpoint($"/swagger/{groupName}/swagger.json", groupName.ToUpperInvariant());
                    }
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseApiVersioning();

            // This middleware has to be configured after 'UseRouting' for 'AllowAnonymousAttribute' to work.
            app.UseMiddleware<JwtTokenMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Health check endpoints must allow anonymous access so we can use them with Azure monitoring:
                // https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check#authentication-and-security
                endpoints.MapHealthChecks("/monitor/live", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Equals("self"),
                }).WithMetadata(new AllowAnonymousAttribute());

                endpoints.MapHealthChecks("/monitor/ready", new HealthCheckOptions
                {
                    Predicate = r => !r.Name.Equals("self"),
                }).WithMetadata(new AllowAnonymousAttribute());
            });
        }
    }
}
