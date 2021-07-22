using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Refit.Extensions.Exceptions
{
    public class DefaultApiExceptionFactory
    {
        static readonly Task<Exception?> NullTask = Task.FromResult<Exception?>(null);

        readonly RefitSettings refitSettings;

        public DefaultApiExceptionFactory(RefitSettings refitSettings)
        {
            this.refitSettings = refitSettings;
        }

        public Task<Exception?> CreateAsync(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                return CreateExceptionAsync(responseMessage, refitSettings)!;
            }
            else
            {
                return NullTask;
            }
        }

        static async Task<Exception> CreateExceptionAsync(HttpResponseMessage responseMessage,
            RefitSettings refitSettings)
        {
            var requestMessage = responseMessage.RequestMessage!;
            var method = requestMessage.Method;

            return await ApiException.Create(requestMessage, method, responseMessage, refitSettings)
                .ConfigureAwait(false);
        }
    }
}
