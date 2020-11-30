using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DummyBitbucketServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/hello", async context => { await context.Response.WriteAsync("Hello World!"); });
                endpoints.Map("site/oauth2/access_token", async context =>
                {
                    Console.WriteLine("In OAuth endpoint");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        @"{""scopes"": ""repository:write"", " +
                        @"""access_token"": ""JNr15MmRZDleiviFEAPlywjQVs5ycbZC3ZjAoboVYNRWfsZdqZnYTGr3PgOCj1JPqxQLv6JdDErdBGp21no="", " +
                        @"""expires_in"": 7200, " +
                        @"""token_type"": ""bearer"", " +
                        @"""state"": ""client_credentials"", " +
                        @"""refresh_token"": ""65ckN6pWu2DeUEqEGk""}");
                });
                endpoints.MapFallback(context => Task.CompletedTask);
            });
        }
    }
}
