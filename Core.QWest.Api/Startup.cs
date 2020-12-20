using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Core.QWest.Api {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers()
                .AddNewtonsoftJson(options => { 

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.Use(async (context, next) => {
                try {
                    var cookies = context.Request.Cookies.Where(x => x.Key == "sessionCookie").ToList();
                    if (cookies.Count == 0) {
                        await next.Invoke();
                        return;
                    }
                    var cookie = cookies[0].Value;
                    var value = cookie;
                    if (value == "") {
                        await next.Invoke();
                        return;
                    }
                    if (value.First() == '"' && value.Last() == '"') {
                        value = value.Substring(1, value.Length - 2);
                    }
                    User user = await DAO.User.GetBySessionCookie(value);
                    if (user == null) {
                        await next.Invoke();
                        return;
                    }
                    context.Items.Add("user", user);
                }
                catch (Exception) { }
                await next.Invoke();
            });

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
