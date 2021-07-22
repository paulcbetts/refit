using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using RichardSzalay.MockHttp;

using Xunit; // for the code gen

namespace Refit.Tests.Extensions.Exceptions
{
    public class ExceptionFactoryTests
    {
        public interface IMyService
        {
            [Get("/get-with-result")]
            Task<string> GetWithResult();

            [Put("/put-without-result")]
            Task PutWithoutResult();
        }

        [Fact]
        public async Task ProvideFactoryWhichAlwaysReturnsNull_WithResult()
        {
            var handler = new MockHttpMessageHandler();
            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler,
                ExceptionFactory = _ => Task.FromResult<Exception>(null)
            };

            handler.Expect(HttpMethod.Get, "http://api/get-with-result")
                   .Respond(HttpStatusCode.NotFound, new StringContent("error-result"));

            var fixture = RestService.For<IMyService>("http://api", settings);

            var result = await fixture.GetWithResult();

            handler.VerifyNoOutstandingExpectation();

            Assert.Equal("error-result", result);
        }

        [Fact]
        public async Task ProvideFactoryWhichAlwaysReturnsNull_WithoutResult()
        {
            var handler = new MockHttpMessageHandler();
            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler,
                ExceptionFactory = _ => Task.FromResult<Exception>(null)
            };

            handler.Expect(HttpMethod.Put, "http://api/put-without-result")
                   .Respond(HttpStatusCode.NotFound);

            var fixture = RestService.For<IMyService>("http://api", settings);

            await fixture.PutWithoutResult();

            handler.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ProvideFactoryWhichAlwaysReturnsException_WithResult()
        {
            var handler = new MockHttpMessageHandler();
            var exception = new Exception("I like to fail");
            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler,
                ExceptionFactory = _ => Task.FromResult<Exception>(exception)
            };

            handler.Expect(HttpMethod.Get, "http://api/get-with-result")
                   .Respond(HttpStatusCode.OK, new StringContent("success-result"));

            var fixture = RestService.For<IMyService>("http://api", settings);

            var thrownException = await Assert.ThrowsAsync<Exception>(() => fixture.GetWithResult());
            Assert.Equal(exception, thrownException);

            handler.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ProvideFactoryWhichAlwaysReturnsException_WithoutResult()
        {
            var handler = new MockHttpMessageHandler();
            var exception = new Exception("I like to fail");
            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler,
                ExceptionFactory = _ => Task.FromResult<Exception>(exception)
            };

            handler.Expect(HttpMethod.Put, "http://api/put-without-result")
                   .Respond(HttpStatusCode.OK);

            var fixture = RestService.For<IMyService>("http://api", settings);

            var thrownException = await Assert.ThrowsAsync<Exception>(() => fixture.PutWithoutResult());
            Assert.Equal(exception, thrownException);

            handler.VerifyNoOutstandingExpectation();
        }
    }
}
