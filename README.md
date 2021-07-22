![Refit](refit_logo.png)

## Refit: The automatic type-safe REST library for .NET Core, Xamarin and .NET

[![Build Status](https://dev.azure.com/dotnet/ReactiveUI/_apis/build/status/Refit-CI?branchName=master)](https://dev.azure.com/dotnet/ReactiveUI/_build/latest?definitionId=17)

||Refit|Refit.HttpClientFactory|Refit.Newtonsoft.Json|
|-|-|-|-|
|*NuGet*|[![NuGet](https://img.shields.io/nuget/v/Refit.svg)](https://www.nuget.org/packages/Refit/)|[![NuGet](https://img.shields.io/nuget/v/Refit.HttpClientFactory.svg)](https://www.nuget.org/packages/Refit.HttpClientFactory/)|[![NuGet](https://img.shields.io/nuget/v/Refit.Newtonsoft.Json.svg)](https://www.nuget.org/packages/Refit.Newtonsoft.Json/)|
|*Azure<br />Artifacts*|[![Refit package in Refit feed in Azure Artifacts](https://azpkgsshield.azurevoodoo.net/dotnet/ReactiveUI/Refit/Refit)](https://dev.azure.com/dotnet/ReactiveUI/_packaging?_a=package&feed=6368ad76-9653-4c2f-a6c6-da8c790ae826&package=2f65cb3b-df09-4050-a84e-b868ed95fd28&preferRelease=true)|[![Refit.HttpClientFactory package in Refit feed in Azure Artifacts](https://azpkgsshield.azurevoodoo.net/dotnet/ReactiveUI/Refit/Refit.HttpClientFactory)](https://dev.azure.com/dotnet/ReactiveUI/_packaging?_a=package&feed=6368ad76-9653-4c2f-a6c6-da8c790ae826&package=d87934cb-2f7b-44b7-83b8-3872ac965ef2&preferRelease=true)|[![Refit.Newtonsoft.Json package in Refit feed in Azure Artifacts](https://azpkgsshield.azurevoodoo.net/dotnet/ReactiveUI/Refit/Refit.Newtonsoft.Json)](https://dev.azure.com/dotnet/ReactiveUI/_packaging?_a=package&feed=6368ad76-9653-4c2f-a6c6-da8c790ae826&package=d87934cb-2f7b-44b7-83b8-3872ac965ef2&preferRelease=true)|

CI Feed: `https://pkgs.dev.azure.com/dotnet/ReactiveUI/_packaging/Refit/nuget/v3/index.json`

Refit is a library heavily inspired by Square's
[Retrofit](http://square.github.io/retrofit) library, and it turns your REST
API into a live interface:

```csharp
public interface IGitHubApi
{
    [Get("/users/{user}")]
    Task<User> GetUser(string user);
}
```

The `RestService` class generates an implementation of `IGitHubApi` that uses
`HttpClient` to make its calls:

```csharp
var gitHubApi = RestService.For<IGitHubApi>("https://api.github.com");
var octocat = await gitHubApi.GetUser("octocat");
```
.NET Core supports registering via HttpClientFactory
```csharp
services
    .AddRefitClient<IGitHubApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"));
```

# Table of Contents

* [Where does this work?](#where-does-this-work)
  * [Breaking changes in 6.x](#breaking-changes-in-6x)
* [API Attributes](#api-attributes)
* [Dynamic Querystring Parameters](#dynamic-querystring-parameters)
* [Collections as Querystring parameters](#collections-as-querystring-parameters)
* [Unescape Querystring parameters](#unescape-querystring-parameters)
* [Body content](#body-content)
  * [Buffering and the Content-Length header](#buffering-and-the-content-length-header)
  * [JSON content](#json-content)
  * [XML Content](#xml-content)
  * [Form posts](#form-posts)
* [Setting request headers](#setting-request-headers)
  * [Static headers](#static-headers)
  * [Dynamic headers](#dynamic-headers)
  * [Bearer Authentication](#bearer-authentication)
  * [Reducing header boilerplate with DelegatingHandlers (Authorization headers worked example)](#reducing-header-boilerplate-with-delegatinghandlers-authorization-headers-worked-example)
  * [Redefining headers](#redefining-headers)
  * [Removing headers](#removing-headers)
* [Passing state into HttpRequestMessage.Properties](#passing-state-into-httprequestmessageproperties)
  * [Via the \[Property\] attribute](#via-the-property-attribute)
  * [Via RefitSettings.PropertyProviders](#via-refitsettingspropertyproviders)
    * [RefitTargetInterfaceTypePropertyProvider](#refittargetinterfacetypepropertyprovider)
    * [MethodInfoPropertyProvider](#methodinfopropertyprovider)
    * [CustomAttributePropertyProvider](#customattributepropertyprovider)
    * [Extending Refit by writing your own PropertyProvider](#extending-refit-by-writing-your-own-propertyprovider)
    * [Support for Polly and Polly.Context](#support-for-polly-and-pollycontext)
* [Multipart uploads](#multipart-uploads)
* [Retrieving the response](#retrieving-the-response)
* [Using generic interfaces](#using-generic-interfaces)
* [Interface inheritance](#interface-inheritance)
  * [Headers inheritance](#headers-inheritance)
* [Default Interface Methods](#default-interface-methods)
* [Using HttpClientFactory](#using-httpclientfactory)
* [Handling exceptions](#handling-exceptions)
  * [When returning Task<ApiResponse<T>>](#when-returning-taskapiresponset)
  * [When returning Task<T>](#when-returning-taskt)
  * [Providing a custom ExceptionFactory](#providing-a-custom-exceptionfactory)

### Where does this work?

Refit currently supports the following platforms and any .NET Standard 2.0 target:

* UWP
* Xamarin.Android
* Xamarin.Mac
* Xamarin.iOS
* Desktop .NET 4.6.1
* .NET 5 / .NET Core
* Blazor
* Uno Platform

### SDK Requirements

Refit 6 requires Visual Studio 16.8 or higher, or the .NET SDK 5.0.100 or higher. It can target any .NET Standard 2.0 platform.

Refit 6 does not support the old `packages.config` format for NuGet references (as they do not support analyzers/source generators). You must
[migrate to PackageReference](https://devblogs.microsoft.com/nuget/migrate-packages-config-to-package-reference/) to use Refit v6 and later.

#### Breaking changes in 6.x

Refit 6 makes [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview) the default JSON serializer. If you'd like to continue to use `Newtonsoft.Json`, add the `Refit.Newtonsoft.Json` NuGet package and set your `ContentSerializer` to `NewtonsoftJsonContentSerializer` on your `RefitSettings` instance. `System.Text.Json` is faster and uses less memory, though not all features are supported. The [migration guide](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0) contains more details.

`IContentSerializer` was renamed to `IHttpContentSerializer` to better reflect its purpose. Additionally, two of its methods were renamed, `SerializeAsync<T>` -> `ToHttpContent<T>` and `DeserializeAsync<T>` -> `FromHttpContentAsync<T>`. Any existing implementations of these will need to be updated, though the changes should be minor.

### API Attributes

Every method must have an HTTP attribute that provides the request method and
relative URL. There are six built-in annotations: Get, Post, Put, Delete, Patch and
Head. The relative URL of the resource is specified in the annotation.

```csharp
[Get("/users/list")]
```

You can also specify query parameters in the URL:

```csharp
[Get("/users/list?sort=desc")]
```

A request URL can be updated dynamically using replacement blocks and
parameters on the method. A replacement block is an alphanumeric string
surrounded by { and }.

If the name of your parameter doesn't match the name in the URL path, use the
`AliasAs` attribute.

```csharp
[Get("/group/{id}/users")]
Task<List<User>> GroupList([AliasAs("id")] int groupId);
```

A request url can also bind replacement blocks to a custom object

```csharp
[Get("/group/{request.groupId}/users/{request.userId}")]
Task<List<User>> GroupList(UserGroupRequest request);

class UserGroupRequest{
    int groupId { get;set; }
    int userId { get;set; }
}

```

Parameters that are not specified as a URL substitution will automatically be
used as query parameters. This is different than Retrofit, where all
parameters must be explicitly specified.

The comparison between parameter name and URL parameter is *not*
case-sensitive, so it will work correctly if you name your parameter `groupId`
in the path `/group/{groupid}/show` for example.

```csharp
[Get("/group/{id}/users")]
Task<List<User>> GroupList([AliasAs("id")] int groupId, [AliasAs("sort")] string sortOrder);

GroupList(4, "desc");
>>> "/group/4/users?sort=desc"
```

Round-tripping route parameter syntax: Forward slashes aren't encoded when using a double-asterisk (\*\*) catch-all parameter syntax.

During link generation, the routing system encodes the value captured in a double-asterisk (\*\*) catch-all parameter (for example, {**myparametername}) except the forward slashes.

The type of round-tripping route parameter must be string.

```csharp
[Get("/search/{**page}")]
Task<List<Page>> Search(string page);

Search("admin/products");
>>> "/search/admin/products"
```

### Dynamic Querystring Parameters

If you specify an `object` as a query parameter, all public properties which are not null are used as query parameters.
This previously only applied to GET requests, but has now been expanded to all HTTP request methods, partly thanks to Twitter's hybrid API that insists on non-GET requests with querystring parameters.
Use the `Query` attribute the change the behavior to 'flatten' your query parameter object. If using this Attribute you can specify values for the Delimiter and the Prefix which are used to 'flatten' the object.

```csharp
public class MyQueryParams
{
    [AliasAs("order")]
    public string SortOrder { get; set; }

    public int Limit { get; set; }

    public KindOptions Kind { get; set; }
}

public enum KindOptions
{
    Foo,

    [EnumMember(Value = "bar")]
    Bar
}


[Get("/group/{id}/users")]
Task<List<User>> GroupList([AliasAs("id")] int groupId, MyQueryParams params);

[Get("/group/{id}/users")]
Task<List<User>> GroupListWithAttribute([AliasAs("id")] int groupId, [Query(".","search")] MyQueryParams params);


params.SortOrder = "desc";
params.Limit = 10;
params.Kind = KindOptions.Bar;

GroupList(4, params)
>>> "/group/4/users?order=desc&Limit=10&Kind=bar"

GroupListWithAttribute(4, params)
>>> "/group/4/users?search.order=desc&search.Limit=10&search.Kind=bar"
```

A similar behavior exists if using a Dictionary, but without the advantages of the `AliasAs` attributes and of course no intellisense and/or type safety.

You can also specify querystring parameters with [Query] and have them flattened in non-GET requests, similar to:
```csharp
[Post("/statuses/update.json")]
Task<Tweet> PostTweet([Query]TweetParams params);
```

Where `TweetParams` is a POCO, and properties will also support `[AliasAs]` attributes.

### Collections as Querystring parameters

Use the `Query` attribute to specify format in which collections should be formatted in query string

```csharp
[Get("/users/list")]
Task Search([Query(CollectionFormat.Multi)]int[] ages);

Search(new [] {10, 20, 30})
>>> "/users/list?ages=10&ages=20&ages=30"

[Get("/users/list")]
Task Search([Query(CollectionFormat.Csv)]int[] ages);

Search(new [] {10, 20, 30})
>>> "/users/list?ages=10%2C20%2C30"
```

You can also specify collection format in `RefitSettings`, that will be used by default, unless explicitly defined in `Query` attribute.

```csharp
var gitHubApi = RestService.For<IGitHubApi>("https://api.github.com",
    new RefitSettings {
        CollectionFormat = CollectionFormat.Multi
    });
```

### Unescape Querystring parameters

Use the `QueryUriFormat` attribute to specify if the query parameters should be url escaped

```csharp
[Get("/query")]
[QueryUriFormat(UriFormat.Unescaped)]
Task Query(string q);

Query("Select+Id,Name+From+Account")
>>> "/query?q=Select+Id,Name+From+Account"
```

### Body content

One of the parameters in your method can be used as the body, by using the
Body attribute:

```csharp
[Post("/users/new")]
Task CreateUser([Body] User user);
```

There are four possibilities for supplying the body data, depending on the
type of the parameter:

* If the type is `Stream`, the content will be streamed via `StreamContent`
* If the type is `string`, the string will be used directly as the content unless `[Body(BodySerializationMethod.Json)]` is set which will send it as a `StringContent`
* If the parameter has the attribute `[Body(BodySerializationMethod.UrlEncoded)]`,
  the content will be URL-encoded (see [form posts](#form-posts) below)
* For all other types, the object will be serialized using the content serializer specified in
RefitSettings (JSON is the default).

#### Buffering and the `Content-Length` header

By default, Refit streams the body content without buffering it. This means you can
stream a file from disk, for example, without incurring the overhead of loading
the whole file into memory. The downside of this is that no `Content-Length` header
is set _on the request_. If your API needs you to send a `Content-Length` header with
the request, you can disable this streaming behavior by setting the `buffered` argument
of the `[Body]` attribute to `true`:

```csharp
Task CreateUser([Body(buffered: true)] User user);
```

#### JSON content

JSON requests and responses are serialized/deserialized using an instance of the `IHttpContentSerializer` interface. Refit provides two implementations out of the box: `SystemTextJsonContentSerializer` (which is the default JSON serializer) and `NewtonsoftJsonContentSerializer`. The first uses `System.Text.Json` APIs and is focused on high performance and low memory usage, while the latter uses the known `Newtonsoft.Json` library and is more versatile and customizable. You can read more about the two serializers and the main differences between the two [at this link](https://docs.microsoft.com/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to).

For instance, here is how to create a new `RefitSettings` instance using the `Newtonsoft.Json`-based serializer (you'll also need to add a `PackageReference` to `Refit.Newtonsoft.Json`):

```csharp
var settings = new RefitSettings(new NewtonsoftJsonContentSerializer());
```

If you're using `Newtonsoft.Json` APIs, you can customize their behavior by setting the `Newtonsoft.Json.JsonConvert.DefaultSettings` property:

```csharp
JsonConvert.DefaultSettings =
    () => new JsonSerializerSettings() {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = {new StringEnumConverter()}
    };

// Serialized as: {"day":"Saturday"}
await PostSomeStuff(new { Day = DayOfWeek.Saturday });
```

As these are global settings they will affect your entire application. It
might be beneficial to isolate the settings for calls to a particular API.
When creating a Refit generated live interface, you may optionally pass a
`RefitSettings` that will allow you to specify what serializer settings you
would like. This allows you to have different serializer settings for separate
APIs:

```csharp
var gitHubApi = RestService.For<IGitHubApi>("https://api.github.com",
    new RefitSettings {
        ContentSerializer = new NewtonsoftJsonContentSerializer(
            new JsonSerializerSettings {
                ContractResolver = new SnakeCasePropertyNamesContractResolver()
        }
    )});

var otherApi = RestService.For<IOtherApi>("https://api.example.com",
    new RefitSettings {
        ContentSerializer = new NewtonsoftJsonContentSerializer(
            new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
        }
    )});
```

Property serialization/deserialization can be customised using Json.NET's
JsonProperty attribute:

```csharp
public class Foo
{
    // Works like [AliasAs("b")] would in form posts (see below)
    [JsonProperty(PropertyName="b")]
    public string Bar { get; set; }
}
```

#### XML Content

XML requests and responses are serialized/deserialized using _System.Xml.Serialization.XmlSerializer_.
By default, Refit will use JSON content serialization, to use XML content configure the ContentSerializer to use the `XmlContentSerializer`:

```csharp
var gitHubApi = RestService.For<IXmlApi>("https://www.w3.org/XML",
    new RefitSettings {
        ContentSerializer = new XmlContentSerializer()
    });
```

Property serialization/deserialization can be customised using   attributes found in the _System.Xml.Serialization_ namespace:

```csharp
    public class Foo
    {
        [XmlElement(Namespace = "https://www.w3.org/XML")]
        public string Bar { get; set; }
    }
```

The _System.Xml.Serialization.XmlSerializer_ provides many options for serializing, those options can be set by providing an `XmlContentSerializerSettings` to the `XmlContentSerializer` constructor:

```csharp
var gitHubApi = RestService.For<IXmlApi>("https://www.w3.org/XML",
    new RefitSettings {
        ContentSerializer = new XmlContentSerializer(
            new XmlContentSerializerSettings
            {
                XmlReaderWriterSettings = new XmlReaderWriterSettings()
                {
                    ReaderSettings = new XmlReaderSettings
                    {
                        IgnoreWhitespace = true
                    }
                }
            }
        )
    });
```

#### <a name="form-posts"></a>Form posts

For APIs that take form posts (i.e. serialized as `application/x-www-form-urlencoded`),
initialize the Body attribute with `BodySerializationMethod.UrlEncoded`.

The parameter can be an `IDictionary`:

```csharp
public interface IMeasurementProtocolApi
{
    [Post("/collect")]
    Task Collect([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
}

var data = new Dictionary<string, object> {
    {"v", 1},
    {"tid", "UA-1234-5"},
    {"cid", new Guid("d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c")},
    {"t", "event"},
};

// Serialized as: v=1&tid=UA-1234-5&cid=d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c&t=event
await api.Collect(data);
```

Or you can just pass any object and all _public, readable_ properties will
be serialized as form fields in the request. This approach allows you to alias
property names using `[AliasAs("whatever")]` which can help if the API has
cryptic field names:

```csharp
public interface IMeasurementProtocolApi
{
    [Post("/collect")]
    Task Collect([Body(BodySerializationMethod.UrlEncoded)] Measurement measurement);
}

public class Measurement
{
    // Properties can be read-only and [AliasAs] isn't required
    public int v { get { return 1; } }

    [AliasAs("tid")]
    public string WebPropertyId { get; set; }

    [AliasAs("cid")]
    public Guid ClientId { get; set; }

    [AliasAs("t")]
    public string Type { get; set; }

    public object IgnoreMe { private get; set; }
}

var measurement = new Measurement {
    WebPropertyId = "UA-1234-5",
    ClientId = new Guid("d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c"),
    Type = "event"
};

// Serialized as: v=1&tid=UA-1234-5&cid=d1e9ea6b-2e8b-4699-93e0-0bcbd26c206c&t=event
await api.Collect(measurement);
```

If you have a type that has `[JsonProperty(PropertyName)]` attributes setting property aliases, Refit will use those too (`[AliasAs]` will take precedence where you have both).
This means that the following type will serialize as `one=value1&two=value2`:

```csharp

public class SomeObject
{
    [JsonProperty(PropertyName = "one")]
    public string FirstProperty { get; set; }

    [JsonProperty(PropertyName = "notTwo")]
    [AliasAs("two")]
    public string SecondProperty { get; set; }
}

```

**NOTE:** This use of `AliasAs` applies to querystring parameters and form body posts, but not to response objects; for aliasing fields on response objects, you'll still need to use `[JsonProperty("full-property-name")]`.

### Setting request headers

#### Static headers

You can set one or more static request headers for a request applying a `Headers`
attribute to the method:

```csharp
[Headers("User-Agent: Awesome Octocat App")]
[Get("/users/{user}")]
Task<User> GetUser(string user);
```

Static headers can also be added to _every request in the API_ by applying the
`Headers` attribute to the interface:

```csharp
[Headers("User-Agent: Awesome Octocat App")]
public interface IGitHubApi
{
    [Get("/users/{user}")]
    Task<User> GetUser(string user);

    [Post("/users/new")]
    Task CreateUser([Body] User user);
}
```

#### Dynamic headers

If the content of the header needs to be set at runtime, you can add a header
with a dynamic value to a request by applying a `Header` attribute to a parameter:

```csharp
[Get("/users/{user}")]
Task<User> GetUser(string user, [Header("Authorization")] string authorization);

// Will add the header "Authorization: token OAUTH-TOKEN" to the request
var user = await GetUser("octocat", "token OAUTH-TOKEN");
```

Adding an `Authorization` header is such a common use case that you can add an access token to a request by applying an `Authorize` attribute to a parameter and optionally specifying the scheme:

```csharp
[Get("/users/{user}")]
Task<User> GetUser(string user, [Authorize("Bearer")] string token);

// Will add the header "Authorization: Bearer OAUTH-TOKEN}" to the request
var user = await GetUser("octocat", "OAUTH-TOKEN");

//note: the scheme defaults to Bearer if none provided
```

If you need to set multiple headers at runtime, you can add a `IDictionary<string, string>`
and apply a `HeaderCollection` attribute to the parameter and it will inject the headers into the request:

[//]: # ({% raw %})
```csharp

[Get("/users/{user}")]
Task<User> GetUser(string user, [HeaderCollection] IDictionary<string, string> headers);

var headers = new Dictionary<string, string> {{"Authorization","Bearer tokenGoesHere"}, {"X-Tenant-Id","123"}};
var user = await GetUser("octocat", headers);
```
[//]: # ({% endraw %})

#### Bearer Authentication

Most APIs need some sort of Authentication. The most common is OAuth Bearer authentication. A header is added to each request of the form: `Authorization: Bearer <token>`. Refit makes it easy to insert your logic to get the token however your app needs, so you don't have to pass a token into each method.

1. Add `[Headers("Authorization: Bearer")]` to the interface or methods which need the token.
2. Set either `AuthorizationHeaderValueGetter` or `AuthorizationHeaderValueWithParamGetter` in the `RefitSettings` instance. The difference is that the latter one passes the `HttpRequestMessage` into the function in case you need to take action based on the specific request. Refit will call your delegate each time it needs to obtain the token, so it's a good idea for your mechanism to cache the token value for some period within the token lifetime.

#### Reducing header boilerplate with DelegatingHandlers (Authorization headers worked example)
Although we make provisions for adding dynamic headers at runtime directly in Refit,
most use-cases would likely benefit from registering a custom `DelegatingHandler` in order to inject the headers as part of the `HttpClient` middleware pipeline
thus removing the need to add lots of `[Header]` or `[HeaderCollection]` attributes.

In the example above we are leveraging a `[HeaderCollection]` parameter to inject an `Authorization` and `X-Tenant-Id` header.
This is quite a common scenario if you are integrating with a 3rd party that uses OAuth2. While it's ok for the occasional endpoint,
it would be quite cumbersome if we had to add that boilerplate to every method in our interface.

In this example we will assume our application is a multi-tenant application that is able to pull information about a tenant through
some interface `ITenantProvider` and has a data store `IAuthTokenStore` that can be used to retrieve an auth token to attach to the outbound request.

```csharp

 //Custom delegating handler for adding Auth headers to outbound requests
 class AuthHeaderHandler : DelegatingHandler
 {
     private readonly ITenantProvider tenantProvider;
     private readonly IAuthTokenStore authTokenStore;

    public AuthHeaderHandler(ITenantProvider tenantProvider, IAuthTokenStore authTokenStore)
    {
         this.tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
         this.authTokenStore = authTokenStore ?? throw new ArgumentNullException(nameof(authTokenStore));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await authTokenStore.GetToken();

        //potentially refresh token here if it has expired etc.

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantProvider.GetTenantId());

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

//Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ITenantProvider, TenantProvider>();
    services.AddTransient<IAuthTokenStore, AuthTokenStore>();
    services.AddTransient<AuthHeaderHandler>();

    //this will add our refit api implementation with an HttpClient
    //that is configured to add auth headers to all requests

    //note: AddRefitClient<T> requires a reference to Refit.HttpClientFactory
    //note: the order of delegating handlers is important and they run in the order they are added!

    services.AddRefitClient<ISomeThirdPartyApi>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"))
        .AddHttpMessageHandler<AuthHeaderHandler>();
        //you could add Polly here to handle HTTP 429 / HTTP 503 etc
}

//Your application code
public class SomeImportantBusinessLogic
{
    private ISomeThirdPartyApi thirdPartyApi;

    public SomeImportantBusinessLogic(ISomeThirdPartyApi thirdPartyApi)
    {
        this.thirdPartyApi = thirdPartyApi;
    }

    public async Task DoStuffWithUser(string username)
    {
        var user = await thirdPartyApi.GetUser(username);
        //do your thing
    }
}
```

If you aren't using dependency injection then you could achieve the same thing by doing something like this:

```csharp
var api = RestService.For<ISomeThirdPartyApi>(new HttpClient(new AuthHeaderHandler(tenantProvider, authTokenStore))
    {
        BaseAddress = new Uri("https://api.example.com")
    }
);

var user = await thirdPartyApi.GetUser(username);
//do your thing
```

#### Redefining headers

Unlike Retrofit, where headers do not overwrite each other and are all added to
the request regardless of how many times the same header is defined, Refit takes
a similar approach to the approach ASP.NET MVC takes with action filters &mdash;
**redefining a header will replace it**, in the following order of precedence:

* `Headers` attribute on the interface _(lowest priority)_
* `Headers` attribute on the method
* `Header` attribute or `HeaderCollection` attribute on a method parameter _(highest priority)_

```csharp
[Headers("X-Emoji: :rocket:")]
public interface IGitHubApi
{
    [Get("/users/list")]
    Task<List> GetUsers();

    [Get("/users/{user}")]
    [Headers("X-Emoji: :smile_cat:")]
    Task<User> GetUser(string user);

    [Post("/users/new")]
    [Headers("X-Emoji: :metal:")]
    Task CreateUser([Body] User user, [Header("X-Emoji")] string emoji);
}

// X-Emoji: :rocket:
var users = await GetUsers();

// X-Emoji: :smile_cat:
var user = await GetUser("octocat");

// X-Emoji: :trollface:
await CreateUser(user, ":trollface:");
```

**Note:** This redefining behavior only applies to headers _with the same name_. Headers with different names are not replaced. The following code will result in all headers being included:

```csharp
[Headers("Header-A: 1")]
public interface ISomeApi
{
    [Headers("Header-B: 2")]
    [Post("/post")]
    Task PostTheThing([Header("Header-C")] int c);
}

// Header-A: 1
// Header-B: 2
// Header-C: 3
var user = await api.PostTheThing(3);
```

#### Removing headers

Headers defined on an interface or method can be removed by redefining
a static header without a value (i.e. without `: <value>`) or passing `null` for
a dynamic header. _Empty strings will be included as empty headers._

```csharp
[Headers("X-Emoji: :rocket:")]
public interface IGitHubApi
{
    [Get("/users/list")]
    [Headers("X-Emoji")] // Remove the X-Emoji header
    Task<List> GetUsers();

    [Get("/users/{user}")]
    [Headers("X-Emoji:")] // Redefine the X-Emoji header as empty
    Task<User> GetUser(string user);

    [Post("/users/new")]
    Task CreateUser([Body] User user, [Header("X-Emoji")] string emoji);
}

// No X-Emoji header
var users = await GetUsers();

// X-Emoji:
var user = await GetUser("octocat");

// No X-Emoji header
await CreateUser(user, null);

// X-Emoji:
await CreateUser(user, "");
```

### Passing state into HttpRequestMessage.Properties

Refit is built on top of [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0) which itself allows you to customize its behavior through custom
[HttpClient message handlers](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers). To support this there is a property called `HttpRequestMessage.Properties`
which is intended to allow developers to pass state from calls to `HttpClient` into their `HttpClient` message handlers.

Refit supports this in two ways:
 - Passing dynamic runtime state directly via the `[Property]` attribute on individual Refit client interface methods
 - Customizing `RefitSettings.PropertyProviders` with a list of property providers that each take in the `MethodInfo` of the executing Refit method, and the `Type` of the Refit client interface and return any arbitrary properties.

**Note**: in .NET 5+ `HttpRequestMessage.Properties` has been marked `Obsolete` and Refit will instead populate the value into the new `HttpRequestMessage.Options`.
This documentation refers to `HttpRequestMessage.Properties`, but Refit supports both, and you should use the one that is appropriate for your version of .NET.

#### Via the [Property] attribute
If there is runtime state that you need to pass to a `DelegatingHandler` you can add a property with a dynamic value to the underlying `HttpRequestMessage.Properties`
by applying a `Property` attribute to a parameter:

```csharp
public interface IGitHubApi
{
    [Post("/users/new")]
    Task CreateUser([Body] User user, [Property("SomeKey")] string someValue);

    [Post("/users/new")]
    Task CreateUser([Body] User user, [Property] string someOtherKey);
}
```

The attribute constructor optionally takes a string which becomes the key in the `HttpRequestMessage.Properties` dictionary.
If no key is explicitly defined then the name of the parameter becomes the key.
If a key is defined multiple times the value in `HttpRequestMessage.Properties` will be overwritten.
The parameter itself can be any `object`. Properties can be accessed inside a `DelegatingHandler` as follows:

```csharp
class RequestPropertyHandler : DelegatingHandler
{
    public RequestPropertyHandler(HttpMessageHandler innerHandler = null) : base(innerHandler ?? new HttpClientHandler()) {}

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // See if the request has a the property
        if(request.Properties.ContainsKey("SomeKey")
        {
            var someProperty = request.Properties["SomeKey"];
            //do stuff
        }

        if(request.Properties.ContainsKey("someOtherKey")
        {
            var someOtherProperty = request.Properties["someOtherKey"];
            //do stuff
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
```
#### Via `RefitSettings.PropertyProviders`

Refit provides some `PropertyProvider` implementations out of the box to support common use cases for customising the behavior of the underlying `HttpClient` instance
through `HttpClient` middleware using information placed into `HttpRequestMessage.Properties` by the configured `List<PropertyProvider>`.

A `List<PropertyProvider>` can be configured via `RefitSettings.PropertyProviders` like so:

```
var settings = new RefitSettings
{
    PropertyProviders = PropertyProviderFactory
        .WithPropertyProviders()
        .RefitTargetInterfaceTypePropertyProvider()
        .MethodInfoPropertyProvider()
        .CustomAttributePropertyProvider()
        .PropertyProvider(new MyCustomPropertyProvider())
        .Build()
};
```

If no `PropertyProviders` are specified a `RefitTargetInterfaceTypePropertyProvider` will be configured by default.

If you want to remove the default `PropertyProviders` and have none at all you can do it as follows:

```
var settings = new RefitSettings
{
    PropertyProviders = PropertyProviderFactory.WithoutPropertyProviders();
};
```

**Note**: If a `PropertyProvider` throws an exception the `Exception` instance will be populated into the `HttpRequestMessage.Properties` under the key `HttpMessageRequestOptions.PropertyProviderException`;

##### RefitTargetInterfaceTypePropertyProvider

This property provider adds a property to `HttpRequestMessage.Properties` under the key `HttpMessageRequestOptions.InterfaceType` that contains the target `Type` of the Refit instance.

There may be times when you want to know what the target interface type is of the Refit instance. An example is where you
have a derived interface that implements a common base like this:

```csharp
public interface IGetAPI<TEntity>
{
    [Get("/{key}")]
    Task<TEntity> Get(long key);
}

public interface IUsersAPI : IGetAPI<User>
{
}

public interface IOrdersAPI : IGetAPI<Order>
{
}
```

You can access the concrete type of the interface for use in a handler, such as to alter the URL of the request:

[//]: # ({% raw %})
```csharp
class RequestPropertyHandler : DelegatingHandler
{
    public RequestPropertyHandler(HttpMessageHandler innerHandler = null) : base(innerHandler ?? new HttpClientHandler()) {}

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the type of the target interface
        Type interfaceType = (Type)request.Properties[HttpMessageRequestOptions.InterfaceType];

        var builder = new UriBuilder(request.RequestUri);
        // Alter the Path in some way based on the interface or an attribute on it
        builder.Path = $"/{interfaceType.Name}{builder.Path}";
        // Set the new Uri on the outgoing message
        request.RequestUri = builder.Uri;

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
```
[//]: # ({% endraw %})

##### MethodInfoPropertyProvider

This property provider adds a property to `HttpRequestMessage.Properties` under the key `HttpMessageRequestOptions.MethodInfo` that contains the `MethodInfo` of the currently executing method on the Refit instance.

##### CustomAttributePropertyProvider

This property provider adds properties into `HttpRequestMessage.Properties` for every custom `Attribute` on the currently executing method of the Refit instance,
as well as every custom `Attribute` on the Refit interface itself that is not a subclass of `RefitAttribute`. The key will be the name of the `Attribute` and the value will be the instance of the `Attribute`.

This allows you to extend Refit by decorating your Refit client interface with custom attributes and then making decisions based on that data inside a custom `DelegatingHandler` like:

```
public interface IGitHubApi
{
    [SomeBehavior("someValue")]
    [Get("/users/{name}")]
    Task<User> CreateUser(string name);

    [Post("/users/new")]
    Task CreateUser([Body] User user);
}

class SomeBehaviorHandler : DelegatingHandler
{
    public SomeBehaviorHandler(HttpMessageHandler innerHandler = null) : base(innerHandler ?? new HttpClientHandler()) {}

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the attribute instance
        SomeBehaviorAttribute someBehavior = (SomeBehaviorAttribute)request.Properties[nameof(SomeBehaviorAttribute)];

        //do some custom behavior

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
```

##### Extending Refit by writing your own PropertyProvider

A `PropertyProvider` is simply a class that implements the following interface:

```
public interface PropertyProvider
{
    void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType);
}
```

It takes a `IDictionary<string, object?>` which you can populate properties into, the `MethodInfo` of the currently executing method on the Refit instance, and the target `Type` of the Refit instance.

Next let's take a look at an example of how this could be used.

##### Support for Polly and Polly.Context

Because Refit supports `HttpClientFactory` it is possible to configure [Polly](https://github.com/App-vNext/Polly) policies on your HttpClient.
If your policy makes use of `Polly.Context` this can be passed via Refit by either adding a `[Property]` attribute on your Refit interface method like:

```
public interface IGitHubApi
{
    [Post("/users/new")]
    Task CreateUser([Body] User user, [Property("PollyExecutionContext")] Polly.Context context);
}
```

Or, if you simply wanted to populate an empty `Polly.Context` automatically on every request you could do it with a custom property provider like:

```
public class PollyContextPropertyProvider : PropertyProvider
{
    public void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType)
    {
        properties["PollyExecutionContext"] = new Polly.Context();
    }
}

var settings = new RefitSettings
{
    PropertyProviders = PropertyProviderFactory
        .WithPropertyProviders()
        .PropertyProvider(new PollyContextPropertyProvider())
        .Build()
};
```

### Multipart uploads

Methods decorated with `Multipart` attribute will be submitted with multipart content type.
At this time, multipart methods support the following parameter types:

 - string (parameter name will be used as name and string value as value)
 - byte array
 - Stream
 - FileInfo

Name of the field in the multipart data priority precedence:

* multipartItem.Name if specified and not null (optional); dynamic, allows naming form data part at execution time.
* [AliasAs] attribute  (optional) that decorate the streamPart parameter in the method signature (see below); static, defined in code.
* MultipartItem parameter name (default) as defined in the method signature; static, defined in code.

A custom boundary can be specified with an optional string parameter to the `Multipart` attribute. If left empty, this defaults to `----MyGreatBoundary`.

To specify the file name and content type for byte array (`byte[]`), `Stream` and `FileInfo` parameters, use of a wrapper class is required.
The wrapper classes for these types are `ByteArrayPart`, `StreamPart` and `FileInfoPart`.

```csharp
public interface ISomeApi
{
    [Multipart]
    [Post("/users/{id}/photo")]
    Task UploadPhoto(int id, [AliasAs("myPhoto")] StreamPart stream);
}
```

To pass a Stream to this method, construct a StreamPart object like so:

```csharp
someApiInstance.UploadPhoto(id, new StreamPart(myPhotoStream, "photo.jpg", "image/jpeg"));
```

Note: The AttachmentName attribute that was previously described in this section has been deprecated and its use is not recommended.

### Retrieving the response

Note that in Refit unlike in Retrofit, there is no option for a synchronous
network request - all requests must be async, either via `Task` or via
`IObservable`. There is also no option to create an async method via a Callback
parameter unlike Retrofit, because we live in the async/await future.

Similarly to how body content changes via the parameter type, the return type
will determine the content returned.

Returning Task without a type parameter will discard the content and solely
tell you whether or not the call succeeded:

```csharp
[Post("/users/new")]
Task CreateUser([Body] User user);

// This will throw if the network call fails
await CreateUser(someUser);
```

If the type parameter is 'HttpResponseMessage' or 'string', the raw response
message or the content as a string will be returned respectively.

```csharp
// Returns the content as a string (i.e. the JSON data)
[Get("/users/{user}")]
Task<string> GetUser(string user);

// Returns the raw response, as an IObservable that can be used with the
// Reactive Extensions
[Get("/users/{user}")]
IObservable<HttpResponseMessage> GetUser(string user);
```

There is also a generic wrapper class called `ApiResponse<T>` that can be used as a return type. Using this class as a return type allows you to retrieve not just the content as an object, but also any metadata associated with the request/response. This includes information such as response headers, the http status code and reason phrase (e.g. 404 Not Found), the response version, the original request message that was sent and in the case of an error, an `ApiException` object containing details of the error. Following are some examples of how you can retrieve the response metadata.

```csharp
//Returns the content within a wrapper class containing metadata about the request/response
[Get("/users/{user}")]
Task<ApiResponse<User>> GetUser(string user);

//Calling the API
var response = await gitHubApi.GetUser("octocat");

//Getting the status code (returns a value from the System.Net.HttpStatusCode enumeration)
var httpStatus = response.StatusCode;

//Determining if a success status code was received
if(response.IsSuccessStatusCode)
{
    //YAY! Do the thing...
}

//Retrieving a well-known header value (e.g. "Server" header)
var serverHeaderValue = response.Headers.Server != null ? response.Headers.Server.ToString() : string.Empty;

//Retrieving a custom header value
var customHeaderValue = string.Join(',', response.Headers.GetValues("A-Custom-Header"));

//Looping through all the headers
foreach(var header in response.Headers)
{
    var headerName = header.Key;
    var headerValue = string.Join(',', header.Value);
}

//Finally, retrieving the content in the response body as a strongly-typed object
var user = response.Content;
```

### Using generic interfaces

When using something like ASP.NET Web API, it's a fairly common pattern to have a whole stack of CRUD REST services. Refit now supports these, allowing you to define a single API interface with a generic type:

```csharp
public interface IReallyExcitingCrudApi<T, in TKey> where T : class
{
    [Post("")]
    Task<T> Create([Body] T payload);

    [Get("")]
    Task<List<T>> ReadAll();

    [Get("/{key}")]
    Task<T> ReadOne(TKey key);

    [Put("/{key}")]
    Task Update(TKey key, [Body]T payload);

    [Delete("/{key}")]
    Task Delete(TKey key);
}
```

Which can be used like this:

```csharp
// The "/users" part here is kind of important if you want it to work for more
// than one type (unless you have a different domain for each type)
var api = RestService.For<IReallyExcitingCrudApi<User, string>>("http://api.example.com/users");
```
### Interface inheritance

When multiple services that need to be kept separate share a number of APIs, it is possible to leverage interface inheritance to avoid having to define the same Refit methods multiple times in different services:

```csharp
public interface IBaseService
{
    [Get("/resources")]
    Task<Resource> GetResource(string id);
}

public interface IDerivedServiceA : IBaseService
{
    [Delete("/resources")]
    Task DeleteResource(string id);
}

public interface IDerivedServiceB : IBaseService
{
    [Post("/resources")]
    Task<string> AddResource([Body] Resource resource);
}
```

In this example, the `IDerivedServiceA` interface will expose both the `GetResource` and `DeleteResource` APIs, while `IDerivedServiceB` will expose `GetResource` and `AddResource`.

#### Headers inheritance

When using inheritance, existing header attributes will be passed along as well, and the inner-most ones will have precedence:

```csharp
[Headers("User-Agent: AAA")]
public interface IAmInterfaceA
{
    [Get("/get?result=Ping")]
    Task<string> Ping();
}

[Headers("User-Agent: BBB")]
public interface IAmInterfaceB : IAmInterfaceA
{
    [Get("/get?result=Pang")]
    [Headers("User-Agent: PANG")]
    Task<string> Pang();

    [Get("/get?result=Foo")]
    Task<string> Foo();
}
```

Here, `IAmInterfaceB.Pang()` will use `PANG` as its user agent, while `IAmInterfaceB.Foo` and `IAmInterfaceB.Ping` will use `BBB`.
Note that if `IAmInterfaceB` didn't have a header attribute, `Foo` would then use the `AAA` value inherited from `IAmInterfaceA`.
If an interface is inheriting more than one interface, the order of precedence is the same as the one in which the inherited interfaces are declared:

```csharp
public interface IAmInterfaceC : IAmInterfaceA, IAmInterfaceB
{
    [Get("/get?result=Foo")]
    Task<string> Foo();
}
```

Here `IAmInterfaceC.Foo` would use the header attribute inherited from `IAmInterfaceA`, if present, or the one inherited from `IAmInterfaceB`, and so on for all the declared interfaces.

### Default Interface Methods
Starting with C# 8.0, default interface methods (a.k.a. DIMs) can be defined on interfaces. Refit interfaces can provide additional logic using DIMs, optionally combined with private and/or static helper methods:
```csharp
public interface IApiClient
{
    // implemented by Refit but not exposed publicly
    [Get("/get")]
    internal Task<string> GetInternal();
    // Publicly available with added logic applied to the result from the API call
    public async Task<string> Get()
        => FormatResponse(await GetInternal());
    private static String FormatResponse(string response)
        => $"The response is: {response}";
}
```
The type generated by Refit will implement the method `IApiClient.GetInternal`. If additional logic is required immediately before or after its invocation, it shouldn't be exposed directly and can thus be hidden from consumers by being marked as `internal`.
The default interface method `IApiClient.Get` will be inherited by all types implementing `IApiClient`, including - of course - the type generated by Refit.
Consumers of the `IApiClient` will call the public `Get` method and profit from the additional logic provided in its implementation (optionally, in this case, with the help of the private static helper `FormatResponse`).
To support runtimes without DIM-support (.NET Core 2.x and below or .NET Standard 2.0 and below), two additional types would be required for the same solution.
```csharp
internal interface IApiClientInternal
{
    [Get("/get")]
    Task<string> Get();
}
public interface IApiClient
{
    public Task<string> Get();
}
internal class ApiClient : IApiClient
{
    private readonly IApiClientInternal client;
    public ApiClient(IApiClientInternal client) => this.client = client;
    public async Task<string> Get()
        => FormatResponse(await client.Get());
    private static String FormatResponse(string response)
        => $"The response is: {response}";
}
```

### Using HttpClientFactory

Refit has first class support for the ASP.Net Core 2.1 HttpClientFactory. Add a reference to `Refit.HttpClientFactory` and call
the provided extension method in your `ConfigureServices` method to configure your Refit interface:

```csharp
services.AddRefitClient<IWebApi>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"));
        // Add additional IHttpClientBuilder chained methods as required here:
        // .AddHttpMessageHandler<MyHandler>()
        // .SetHandlerLifetime(TimeSpan.FromMinutes(2));
```

Optionally, a `RefitSettings` object can be included:
```csharp
var settings = new RefitSettings();
// Configure refit settings here

services.AddRefitClient<IWebApi>(settings)
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"));
        // Add additional IHttpClientBuilder chained methods as required here:
        // .AddHttpMessageHandler<MyHandler>()
        // .SetHandlerLifetime(TimeSpan.FromMinutes(2));

// or injected from the container
services.AddRefitClient<IWebApi>(provider => new RefitSettings() { /* configure settings */ })
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"));
        // Add additional IHttpClientBuilder chained methods as required here:
        // .AddHttpMessageHandler<MyHandler>()
        // .SetHandlerLifetime(TimeSpan.FromMinutes(2));

```
Note that some of the properties of `RefitSettings` will be ignored because the `HttpClient` and `HttpClientHandlers` will be managed by the `HttpClientFactory` instead of Refit.

You can then get the api interface using constructor injection:

```csharp
public class HomeController : Controller
{
    public HomeController(IWebApi webApi)
    {
        _webApi = webApi;
    }

    private readonly IWebApi _webApi;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var thing = await _webApi.GetSomethingWeNeed(cancellationToken);
        return View(thing);
    }
}
```

### Handling exceptions
Refit has different exception handling behavior depending on if your Refit interface methods return `Task<T>` or if they return `Task<ApiResponse<T>>`.

#### When returning `Task<ApiResponse<T>>`
Refit traps any `ApiException` raised by the `ExceptionFactory` when processing the response and any errors that occur when attempting to deserialize the response to `Task<ApiResponse<T>>` and populates the exception into the `Error` property on `ApiResponse<T>` without throwing the exception.

You can then decide what to do like so:

    var response = await _myRefitClient.GetSomeStuff();
    if(response.IsSuccessStatusCode)
    {
       //do your thing
    }
    else
    {
       _logger.LogError(response.Error, response.Error.Content);
    }


#### When returning `Task<T>`
Refit throws any `ApiException` raised by the `ExceptionFactory` when processing the response and any errors that occur when attempting to deserialize the response to `Task<T>`.

```csharp
// ...
try
{
   var result = await awesomeApi.GetFooAsync("bar");
}
catch (ApiException exception)
{
   //exception handling
}
// ...
```

Refit can also throw `ValidationApiException` instead which in addition to the information present on `ApiException` also contains `ProblemDetails` when the service implements the [RFC 7807](https://tools.ietf.org/html/rfc7807) specification for problem details and the response content type is `application/problem+json`

For specific information on the problem details of the validation exception, simply catch `ValidationApiException`:

```csharp
// ...
try
{
   var result = await awesomeApi.GetFooAsync("bar");
}
catch (ValidationApiException validationException)
{
   // handle validation here by using validationException.Content,
   // which is type of ProblemDetails according to RFC 7807

   // If the response contains additional properties on the problem details,
   // they will be added to the validationException.Content.Extensions collection.
}
catch (ApiException exception)
{
   // other exception handling
}
// ...
```

#### Providing a custom `ExceptionFactory`

You can also override default exceptions behavior that are raised by the `ExceptionFactory` when processing the result by providing a custom exception factory in `RefitSettings`. For example, you can suppress all exceptions with the following:

```csharp
var nullTask = Task.FromResult<Exception>(null);

var gitHubApi = RestService.For<IGitHubApi>("https://api.github.com",
    new RefitSettings {
        ExceptionFactory = httpResponse => nullTask;
    });
```

Note that exceptions raised when attempting to deserialize the response are not affected by this.