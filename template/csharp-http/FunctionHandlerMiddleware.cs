using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Function;
using Microsoft.AspNetCore.Http;
using OpenFaas.Http.Function.Sdk;

namespace OpenFaas.Http.Function.Server
{
    /// <summary>
    /// Function handler invoking middleware, receives the http request from of-watchdog, invokes the function handler, and sets the http response
    /// as the result of the function.
    /// </summary>
    public class FunctionHandlerMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionHandlerMiddleware"/>
        /// </summary>
        /// <param name="handler"><see cref="FunctionHandler"/> function handler</param>
        public FunctionHandlerMiddleware(IFunctionHandler handler)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <summary>
        /// Gets the function handler
        /// </summary>
        private IFunctionHandler Handler { get; }

        /// <summary>
        /// Invokes the function handler middleware. Passes the request body, headers and query string to the function and writes the results of the fuction invocation
        /// to the response, setting status code, body contents and headers from the function response. This middleware is expected to be the last component in the pipeline
        /// and will not invoke the next request delegate
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> http context</param>
        /// <param name="next"><see cref="RequestDelegate"/> next request delegate in the pipeline.</param>
        /// <returns><see cref="Task"/>.</returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            byte[] contents;

            using (MemoryStream stream = new MemoryStream())
            {
                await context.Request.Body.CopyToAsync(stream);
                contents = stream.ToArray();
            }

            Response response = await Handler.HandleAsync(new Request()
            {
                Body = contents,
                Headers = context.Request.Headers.ToDictionary(s => s.Key, y => y.Value.AsEnumerable()),
                Query = context.Request.Query.ToDictionary(q => q.Key, q => q.Value.AsEnumerable())
            });

            context.Response.StatusCode = response.StatusCode;
            AppendHeaders(context.Response.Headers, response.Headers);
            
            if (response.Body != null && response.Body.Length > 0)
            {
                await context.Response.Body.WriteAsync(response.Body, 0, response.Body.Length);
            }
        }

        /// <summary>
        /// Appends the function response headers to the existing header collection
        /// </summary>
        private void AppendHeaders(IHeaderDictionary headers, Dictionary<string, IEnumerable<string>> functionHeaders)
        {
            foreach (string key in functionHeaders.Keys)
            {
                headers.AppendList<string>(key, functionHeaders[key].ToList());
            }
        }
    }
}
