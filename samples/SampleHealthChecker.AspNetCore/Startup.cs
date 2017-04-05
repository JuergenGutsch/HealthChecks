// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SampleHealthChecker
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks(checks =>
            {
                checks.AddUrlCheck("https://github.com")
                      .AddHealthCheckGroup(
                          "servers",
                          group => group.AddUrlCheck("https://github.com")
                                        .AddUrlCheck("https://google.com")
                                        .AddUrlCheck("https://twitddter.com")
                      )
                      .AddHealthCheckGroup(
                          "memory",
                          group => group.AddPrivateMemorySizeCheck(1)
                                        .AddVirtualMemorySizeCheck(2)
                                        .AddWorkingSetCheck(1),
                          CheckStatus.Unhealthy
                      )
                      .AddCheck("thrower", (Func<IHealthCheckResult>)(() => { throw new DivideByZeroException(); }))
                      .AddCheck("long-running", async cancellationToken => { await Task.Delay(10000, cancellationToken); return HealthCheckResult.Healthy("I ran too long"); });

                checks.AddRetryCheck("possibly failing check", async () => 
                {
                    await Task.Delay(50);
                    return HealthCheckResult.Unhealthy("some retries are failing");
                });
                checks.AddRetryCheck("possibly failing check", builder =>
                {
                    builder.AddUrlCheck("https://github.com");
                });

                /*
                // add valid storage account credentials first
                checks.AddAzureBlobStorageCheck("accountName", "accountKey");
                checks.AddAzureBlobStorageCheck("accountName", "accountKey", "containerName");

                checks.AddAzureTableStorageCheck("accountName", "accountKey");
                checks.AddAzureTableStorageCheck("accountName", "accountKey", "tableName");

                checks.AddAzureFileStorageCheck("accountName", "accountKey");
                checks.AddAzureFileStorageCheck("accountName", "accountKey", "shareName");

                checks.AddAzureQueueStorageCheck("accountName", "accountKey");
                checks.AddAzureQueueStorageCheck("accountName", "accountKey", "queueName");
                */

            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
