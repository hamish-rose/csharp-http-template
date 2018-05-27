using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Function;
using OpenFaas.Http.Function.Sdk;
using Serilog;

namespace OpenFaas.Http.Function.Server
{
   /// <summary>
   /// The entry point of the application, responsible for the starting the web service to listen for
   /// function invocations
   /// </summary>
   public class Program
   {
      /// <summary>
      /// Main method, starts the application
      /// </summary>
      /// <param name="args"><see cref="string[]"/> command line arguments</param>
      /// <returns><see cref="Task"/>.</returns>
      public async static Task Main(string[] args)
      {
         await CreateWebHostBuilder(args).Build().RunAsync();
      }

      /// <summary>
      /// Creates the <see cref="IWebHostBuilder"/> for the application's web host.
      /// </summary>
      /// <param name="args"><see cref="string"/> command line arguments</param>
      /// <returns><see cref="IWebHostBuilder"/> web host builder</returns>
      public static IWebHostBuilder CreateWebHostBuilder(string[] args)
      {
         return new WebHostBuilder()
                    .UseKestrel()
                    .UseSerilog((hostingContext, loggerConfiguration) => 
                    {
                        loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .WriteTo.Console();
                    })
                    .ConfigureServices((IServiceCollection services) =>
                    {
                       services.AddTransient<FunctionHandlerMiddleware>();
                       services.AddTransient<IFunctionHandler, FunctionHandler>();
                    })
                    .Configure(app => app.UseMiddleware<FunctionHandlerMiddleware>());
      }
   }
}
