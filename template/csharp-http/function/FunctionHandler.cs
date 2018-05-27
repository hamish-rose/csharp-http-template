using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFaas.Http.Function.Sdk;

namespace Function
{
    /// <summary>
    /// An example function handler
    /// </summary>
    public class FunctionHandler : IFunctionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionHandler"/> class.
        /// </summary>
        /// <param name="logger"> class logger</param>
        public FunctionHandler(ILogger<FunctionHandler> logger) => Logger = logger;

        /// <summary>
        /// Gets the class logger
        /// </summary>
        private ILogger<FunctionHandler> Logger { get; }

        /// <summary>
        /// Handle the function request
        /// </summary>
        /// <param name="request"><see cref="Request"/> function request</param>
        /// <returns><see cref="Task{Response}"/> function response</returns>
        public Task<Response> HandleAsync(Request request)
        {
            var response = new Response()
            {
                Body = Encoding.UTF8.GetBytes("hello world"),
                StatusCode = 200
            };

            Logger.LogInformation("Received request headers {@headers}", request.Headers);
            Logger.LogInformation("Received request query {@query}", request.Query);

            return Task.FromResult(response);
        }
    }
}
