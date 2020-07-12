using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Text.Unicode;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    var response = context.Response;
                    var multi = new MultipartResponse(response);
                    await multi.SendAsFileBody(GetTestString());

                });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/foo", async context => { await context.Response.WriteAsync("Hello World!"); });
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/testrawstring", async context =>
                {
                    var response = context.Response;
                    var responder = new MultipartResponse(context.Response);
                    await responder.AsBody(GetTestString());
                });
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/testproto", async context =>
                {
                    var response = context.Response;
                    var responder = new MultipartResponse(context.Response);
                    await responder.AsBody(GetTestProtoBytes());
                    
                });
            });
            
            });
        }

        public static string GetTestString()
        {
            return "HELLO WORLD";
        }
        public static PlayerData GetTestProto()
        {
            return new PlayerData(){DataVersion = 100};
        }
        public static byte[] GetTestProtoBytes()
        {
            return GetTestProto().ToByteArray();
        }
    }
}
