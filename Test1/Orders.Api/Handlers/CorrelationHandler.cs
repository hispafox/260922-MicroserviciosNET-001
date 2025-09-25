using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orders.Api.Handlers
{
    public class CorrelationHandler : DelegatingHandler
    {
        private const string CorrelationIdHeaderName = "x-correlation-id";
        private readonly ILogger<CorrelationHandler> _logger;

        public CorrelationHandler(ILogger<CorrelationHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = Activity.Current?.Id ??
                                Activity.Current?.RootId ??
                                Guid.NewGuid().ToString();

            request.Headers.Add(CorrelationIdHeaderName, correlationId);

            _logger.LogDebug("Adding correlation ID {CorrelationId} to request {Method} {Uri}",
                correlationId, request.Method, request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
