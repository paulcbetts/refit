using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Refit.Extensions.Properties;
using RichardSzalay.MockHttp;
using Xunit;

namespace Refit.Tests.Extensions.Properties
{
    public class PropertyProviderTests
    {
        private class HardcodedPropertyProvider : PropertyProvider
        {
            private readonly string key;
            private readonly string value;

            public HardcodedPropertyProvider(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType)
            {
                properties[key] = value;
            }
        }

        private class ExceptionThrowingPropertyProvider : PropertyProvider
        {
            private readonly Exception exception;

            public ExceptionThrowingPropertyProvider(Exception exception)
            {
                this.exception = exception;
            }
            public void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType)
            {
                throw exception;
            }
        }

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
        private class RetryAttribute : Attribute
        {
            public int Times { get; }

            public RetryAttribute(int times) => Times = times;

            protected bool Equals(RetryAttribute other)
            {
                return base.Equals(other) && Times == other.Times;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((RetryAttribute) obj);
            }
        }

        public class MyDummyObject
        {
            protected bool Equals(MyDummyObject other)
            {
                return SomeValue == other.SomeValue && AnotherValue == other.AnotherValue;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MyDummyObject)obj);
            }

            public string SomeValue { get; set; }
            public int AnotherValue { get; set; }
        }

        private const int RetryTargetInterface = 0;
        private const int RetryPostMutlipart = 1;
        private const int RetryPutHeaders = 2;
        private const int RetryGetApiResponse = 3;
        private const int RetryDeleteQueryUriFormat = 4;

        private const string UrlPostMultipart = "post-multipart";
        private const string UrlPutHeaders = "put-headers";
        private const string UrlGetApiResponse = "get-api-response";
        private const string UrlDeleteQueryUriFormat = "delete-query-uri-format";
        private const string UrlHeadInterface = "head-interface";

        private const string ParamPropertyKey = "somePropertyKey";
        private const string ParamPropertyValue = "somePropertyValue";
        /*
         * This is a sample of how an end user might use a property provider
         * to feed metadata into a delegating handler in order to control certain behaviors.
         * Polly's integration with HttpClient via HttpClientFactory allows you to set behaviors
         * that apply across the board to an entire HttpClient, but sometimes that isn't granular enough.
         * This feature provides the possibility of more fine-grained control.
         * You could for example still define policies with Polly and store them in a policy registry
         * and then leverage the CustomAttributePropertyProvider in conjunction with an attribute and
         * DelegatingHandler that allows you to select which Polly policy to apply!
         */
        [Retry(RetryTargetInterface)]
        public interface IMyService
        {
            [Retry(RetryGetApiResponse)]
            [Get("/" + UrlGetApiResponse)]
            Task<ApiResponse<MyDummyObject>> GetTaskApiResponseT([Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryGetApiResponse)]
            [Get("/" + UrlGetApiResponse)]
            Task<MyDummyObject> GetTaskT([Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryPostMutlipart)]
            [Multipart]
            [Post("/" + UrlPostMultipart)]
            Task<ApiResponse<MyDummyObject>> PostMultipartTaskApiResponseT(string multipartValue, [Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryPostMutlipart)]
            [Multipart]
            [Post("/" + UrlPostMultipart)]
            Task<MyDummyObject> PostMultipartTaskT(string multipartValue, [Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryPutHeaders)]
            [Headers("User-Agent: AAA")]
            [Put("/" + UrlPutHeaders)]
            Task<ApiResponse<MyDummyObject>> PutHeadersTaskApiResponseT([Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryPutHeaders)]
            [Headers("User-Agent: AAA")]
            [Put("/" + UrlPutHeaders)]
            Task<MyDummyObject> PutHeadersTaskT([Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryDeleteQueryUriFormat)]
            [QueryUriFormat(UriFormat.Unescaped)]
            [Delete("/" + UrlDeleteQueryUriFormat)]
            Task<ApiResponse<MyDummyObject>> DeleteQueryUriFormatTaskApiResponseT([Property(ParamPropertyKey)] string someProperty);

            [Retry(RetryDeleteQueryUriFormat)]
            [QueryUriFormat(UriFormat.Unescaped)]
            [Delete("/" + UrlDeleteQueryUriFormat)]
            Task<MyDummyObject> DeleteQueryUriFormatTaskT([Property(ParamPropertyKey)] string someProperty);

            [Head("/" + UrlHeadInterface)]
            Task<ApiResponse<MyDummyObject>> HeadInterfaceTaskApiResponseT([Property(ParamPropertyKey)] string someProperty);

            [Head("/" + UrlHeadInterface)]
            Task<MyDummyObject> HeadInterfaceTaskT([Property(ParamPropertyKey)] string someProperty);
        }

        /// <summary>
        /// Verify that CustomAttributePropertyProvider populates all custom attributes into the request properties that are not a subclass of RefitAttribute
        /// </summary>
        public static IEnumerable<object[]> CustomAttributePropertyProviderTestData()
        {
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponseT;
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT;
            var propertyProviders = PropertyProviderFactory
                .WithPropertyProviders()
                .CustomAttributePropertyProvider()
                .Build();
            IDictionary<string, object> expectedProperties;

            refitMethodTaskApiResponseT = refitClient => refitClient.GetTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.GetTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {nameof(RetryAttribute), new RetryAttribute(RetryGetApiResponse)},
                {ParamPropertyKey, ParamPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Get, UrlGetApiResponse, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };

            refitMethodTaskApiResponseT = refitClient => refitClient.PostMultipartTaskApiResponseT("multipart", ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.PostMultipartTaskT("multipart", ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {nameof(RetryAttribute), new RetryAttribute(RetryPostMutlipart)},
                {ParamPropertyKey, ParamPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Post, UrlPostMultipart, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };

            refitMethodTaskApiResponseT = refitClient => refitClient.PutHeadersTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.PutHeadersTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {nameof(RetryAttribute), new RetryAttribute(RetryPutHeaders)},
                {ParamPropertyKey, ParamPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Put, UrlPutHeaders, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };

            refitMethodTaskApiResponseT = refitClient => refitClient.DeleteQueryUriFormatTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.DeleteQueryUriFormatTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {nameof(RetryAttribute), new RetryAttribute(RetryDeleteQueryUriFormat)},
                {ParamPropertyKey, ParamPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Delete, UrlDeleteQueryUriFormat, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };

            refitMethodTaskApiResponseT = refitClient => refitClient.HeadInterfaceTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.HeadInterfaceTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {nameof(RetryAttribute), new RetryAttribute(RetryTargetInterface)},
                {ParamPropertyKey, ParamPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Head, UrlHeadInterface, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };
        }

        /// <summary>
        /// Verify that the behavior of adding the refit interface type to the properties is overridable and can be nulled out, and a property provider returning null doesn't cause the request to fail
        /// </summary>
        public static IEnumerable<object[]> WithoutPropertyProvidersTestData()
        {
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponseT;
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT;
            var propertyProviders = PropertyProviderFactory.WithoutPropertyProviders();
            IDictionary<string, object> expectedProperties;

            refitMethodTaskApiResponseT = refitClient => refitClient.GetTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.GetTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {ParamPropertyKey, ParamPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Get, UrlGetApiResponse, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };
        }

        /// <summary>
        /// Verify that when a property provider writes a property that already exists the request succeeds and the last write wins
        /// </summary>
        public static IEnumerable<object[]> LastWriteWinsTestData()
        {
            var overwrittenPropertyValue = "aDifferentPropertyValue";
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponseT;
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT;
            var propertyProviders = PropertyProviderFactory
                .WithPropertyProviders()
                .PropertyProvider(new HardcodedPropertyProvider(ParamPropertyKey, overwrittenPropertyValue))
                .Build();
            IDictionary<string, object> expectedProperties;

            refitMethodTaskApiResponseT = refitClient => refitClient.GetTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.GetTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {ParamPropertyKey, overwrittenPropertyValue}
            };
            yield return new object[]
            {
                HttpMethod.Get, UrlGetApiResponse, refitMethodTaskT, refitMethodTaskApiResponseT, propertyProviders, expectedProperties
            };
        }

        /// <summary>
        /// Verify that when a property provider throws an exception, the request still succeeds and the exception is recorded in HttpRequestMessageOptions.PropertyProviderException
        /// </summary>
        public static IEnumerable<object[]> ExceptionThrowingPropertyProviderTestData()
        {
            var exception = new Exception("I'm very unhappy about this...kaboom!");
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponeT;
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT;
            var propertyProviders = PropertyProviderFactory.WithPropertyProviders()
                .PropertyProvider(new ExceptionThrowingPropertyProvider(exception))
                .Build();
            IDictionary<string, object> expectedProperties;

            refitMethodTaskApiResponeT = refitClient => refitClient.GetTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.GetTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {ParamPropertyKey, ParamPropertyValue},
                {HttpRequestMessageOptions.PropertyProviderException, exception}
            };
            yield return new object[]
            {
                HttpMethod.Get, UrlGetApiResponse, refitMethodTaskT, refitMethodTaskApiResponeT, propertyProviders, expectedProperties
            };
        }

        /// <summary>
        /// Verify that properties from the interface method as well as multiple property providers wind up in the request properties
        /// </summary>
        public static IEnumerable<object[]> MultiplePropertyProvidersTestData()
        {
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponeT;
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT;
            var propertyProviders = PropertyProviderFactory.WithPropertyProviders()
                .PropertyProvider(new HardcodedPropertyProvider($"{ParamPropertyKey}2", $"{ParamPropertyValue}2"))
                .PropertyProvider(new HardcodedPropertyProvider($"{ParamPropertyKey}3", $"{ParamPropertyValue}3"))
                .Build();
            IDictionary<string, object> expectedProperties;

            refitMethodTaskApiResponeT = refitClient => refitClient.GetTaskApiResponseT(ParamPropertyValue);
            refitMethodTaskT = refitClient => refitClient.GetTaskT(ParamPropertyValue);
            expectedProperties = new Dictionary<string, object>
            {
                {ParamPropertyKey, ParamPropertyValue},
                {$"{ParamPropertyKey}2", $"{ParamPropertyValue}2"},
                {$"{ParamPropertyKey}3", $"{ParamPropertyValue}3"}
            };
            yield return new object[]
            {
                HttpMethod.Get, UrlGetApiResponse, refitMethodTaskT, refitMethodTaskApiResponeT, propertyProviders, expectedProperties
            };
        }

        [Theory]
        [MemberData(nameof(CustomAttributePropertyProviderTestData))]
        [MemberData(nameof(WithoutPropertyProvidersTestData))]
        [MemberData(nameof(LastWriteWinsTestData))]
        [MemberData(nameof(ExceptionThrowingPropertyProviderTestData))]
        [MemberData(nameof(MultiplePropertyProvidersTestData))]
        public async Task GivenPropertyProvider_WhenInvokeRefitReturningTaskApiResponseT_ExpectedPropertiesPopulated(
            HttpMethod httpMethod,
            string url,
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT,
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponseT,
            List<PropertyProvider> propertyProviders,
            IDictionary<string, object> expectedProperties)
        {
            var dummyObject = new MyDummyObject {SomeValue = "AValue", AnotherValue = 1};

            var handler = new MockHttpMessageHandler();
            var settings = new RefitSettings
            {
                HttpMessageHandlerFactory = () => handler,
                PropertyProviders = propertyProviders
            };

            handler.Expect(httpMethod, $"http://api/{url}")
                .Respond(HttpStatusCode.OK, settings.ContentSerializer.ToHttpContent(dummyObject));
            var fixture = RestService.For<IMyService>("http://api", settings);

            var result = await refitMethodTaskApiResponseT(fixture);

            handler.VerifyNoOutstandingExpectation();
            Assert.Equal(dummyObject, result.Content);
#if NET5_0_OR_GREATER
            Assert.Equal(expectedProperties.Count, result.RequestMessage.Options.Count());

            foreach (var expectedProperty in expectedProperties)
            {
                Assert.True(result.RequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<object?>(expectedProperty.Key), out var actualPropertyValue));
                Assert.Equal(expectedProperty.Value, actualPropertyValue);
            }
#else
            Assert.Equal(expectedProperties.Count, result.RequestMessage.Properties.Count);

            foreach (var expectedProperty in expectedProperties)
            {
                var actualPropertyValue = result.RequestMessage.Properties[expectedProperty.Key];
                Assert.NotNull(actualPropertyValue);
                Assert.Equal(expectedProperty.Value, actualPropertyValue);
            }
#endif
        }

        [Theory]
        [MemberData(nameof(CustomAttributePropertyProviderTestData))]
        [MemberData(nameof(WithoutPropertyProvidersTestData))]
        [MemberData(nameof(LastWriteWinsTestData))]
        [MemberData(nameof(ExceptionThrowingPropertyProviderTestData))]
        [MemberData(nameof(MultiplePropertyProvidersTestData))]
        public async Task GivenPropertyProvider_WhenInvokeRefitReturningTaskT_Success(
            HttpMethod httpMethod,
            string url,
            Func<IMyService, Task<MyDummyObject>> refitMethodTaskT,
            Func<IMyService, Task<ApiResponse<MyDummyObject>>> refitMethodTaskApiResponseT,
            List<PropertyProvider> propertyProviders,
            IDictionary<string, object> expectedProperties)
        {
            var dummyObject = new MyDummyObject {SomeValue = "AValue", AnotherValue = 1};

            var handler = new MockHttpMessageHandler();
            var settings = new RefitSettings
            {
                HttpMessageHandlerFactory = () => handler,
                PropertyProviders = propertyProviders
            };

            handler.Expect(httpMethod, $"http://api/{url}")
                .Respond(HttpStatusCode.OK, settings.ContentSerializer.ToHttpContent(dummyObject));

            var fixture = RestService.For<IMyService>("http://api", settings);

            var result = await refitMethodTaskT(fixture);

            handler.VerifyNoOutstandingExpectation();
            Assert.Equal(dummyObject, result);
        }

        [Fact]
        public async Task GivenMethodInfoPropertyProvider_WhenInvokeRefit_MethodInfoPopulatedIntoProperties()
        {
            var dummyObject = new MyDummyObject {SomeValue = "AValue", AnotherValue = 1};

            var handler = new MockHttpMessageHandler();
            var settings = new RefitSettings
            {
                HttpMessageHandlerFactory = () => handler,
                PropertyProviders = PropertyProviderFactory
                    .WithPropertyProviders()
                    .MethodInfoPropertyProvider()
                    .Build()
            };

            handler.Expect(HttpMethod.Get, $"http://api/{UrlGetApiResponse}")
                .Respond(HttpStatusCode.OK, settings.ContentSerializer.ToHttpContent(dummyObject));

            var fixture = RestService.For<IMyService>("http://api", settings);

            var result2 = await fixture.GetTaskApiResponseT(ParamPropertyValue);

            handler.VerifyNoOutstandingExpectation();

            Assert.Equal(dummyObject, result2.Content);
            Assert.Equal(2, result2.RequestMessage?.Properties.Count);
            var methodInfo = result2.RequestMessage?.Properties[HttpRequestMessageOptions.MethodInfo] as MethodInfo;
            var expectedMethodInfo = fixture.GetType().GetMethod(nameof(fixture.GetTaskApiResponseT));
            Assert.Equal(expectedMethodInfo.Name, methodInfo.Name);
        }
    }
}
