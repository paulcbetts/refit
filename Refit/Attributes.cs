using System;
using System.Net.Http;

namespace Refit
{
    /// <summary>
    /// This is a marker attribute used internally by Refit
    /// </summary>
    public class RefitAttribute : Attribute
    {

    }

    public abstract class HttpMethodAttribute : RefitAttribute
    {
        public HttpMethodAttribute(string path)
        {
            Path = path;
        }

        public abstract HttpMethod Method { get; }

        public virtual string Path
        {
            get;
            protected set;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetAttribute : HttpMethodAttribute
    {
        public GetAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Get; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostAttribute : HttpMethodAttribute
    {
        public PostAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Post; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PutAttribute : HttpMethodAttribute
    {
        public PutAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Put; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DeleteAttribute : HttpMethodAttribute
    {
        public DeleteAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Delete; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PatchAttribute : HttpMethodAttribute
    {
        public PatchAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return new HttpMethod("PATCH"); }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsAttribute : HttpMethodAttribute
    {
        public OptionsAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return new HttpMethod("OPTIONS"); }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HeadAttribute : HttpMethodAttribute
    {
        public HeadAttribute(string path) : base(path) { }

        public override HttpMethod Method
        {
            get { return HttpMethod.Head; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MultipartAttribute : RefitAttribute
    {
        public string BoundaryText { get; private set; }

        public MultipartAttribute(string boundaryText = "----MyGreatBoundary")
        {
            BoundaryText = boundaryText;
        }

    }

    public enum BodySerializationMethod
    {
        /// <summary>
        /// Encodes everything using the ContentSerializer in RefitSettings except for strings. Strings are set as-is
        /// </summary>
        Default,

        /// <summary>
        /// Json encodes everything, including strings
        /// </summary>
        [Obsolete("Use BodySerializationMethod.Serialized instead", false)]
        Json,

        /// <summary>
        /// Form-UrlEncode's the values
        /// </summary>
        UrlEncoded,

        /// <summary>
        /// Encodes everything using the ContentSerializer in RefitSettings
        /// </summary>
        Serialized
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class BodyAttribute : RefitAttribute
    {
        public BodyAttribute()
        {

        }
        public BodyAttribute(bool buffered)
        {
            Buffered = buffered;
        }

        public BodyAttribute(BodySerializationMethod serializationMethod, bool buffered)
        {
            SerializationMethod = serializationMethod;
            Buffered = buffered;
        }

        public BodyAttribute(BodySerializationMethod serializationMethod = BodySerializationMethod.Default)
        {
            SerializationMethod = serializationMethod;
        }



        public bool? Buffered { get; set; }
        public BodySerializationMethod SerializationMethod { get; protected set; } = BodySerializationMethod.Default;
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class AliasAsAttribute : RefitAttribute
    {
        public AliasAsAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    [Obsolete("Use Refit.StreamPart, Refit.ByteArrayPart, Refit.FileInfoPart or if necessary, inherit from Refit.MultipartItem", false)]
    public class AttachmentNameAttribute : RefitAttribute
    {
        public AttachmentNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }
    }

    /// <summary>
    /// Allows you provide a Dictionary of headers to be added to the request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HeaderCollectionAttribute : RefitAttribute
    {

    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class HeadersAttribute : RefitAttribute
    {
        public HeadersAttribute(params string[] headers)
        {
            Headers = headers ?? Array.Empty<string>();
        }

        public string[] Headers { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class HeaderAttribute : RefitAttribute
    {
        public HeaderAttribute(string header)
        {
            Header = header;
        }

        public string Header { get; }
    }

    /// <summary>
    /// Used to store the value in HttpRequestMessage.Properties for further processing in a custom DelegatingHandler.
    /// If a string is supplied to the constructor then it will be used as the key in the HttpRequestMessage.Properties dictionary.
    /// If no key is specified then the key will be defaulted to the name of the parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PropertyAttribute : RefitAttribute
    {
        public PropertyAttribute() { }

        public PropertyAttribute(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Specifies the key under which to store the value on the HttpRequestMessage.Properties dictionary.
        /// </summary>
        public string? Key { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class AuthorizeAttribute : RefitAttribute
    {
        public AuthorizeAttribute(string scheme = "Bearer")
        {
            Scheme = scheme;
        }

        public string Scheme { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)] // Property is to allow for form url encoded data

    public class QueryAttribute : RefitAttribute
    {
        CollectionFormat? collectionFormat;

        public QueryAttribute() { }

        public QueryAttribute(string delimiter)
        {
            Delimiter = delimiter;
        }

        public QueryAttribute(string delimiter, string prefix)
        {
            Delimiter = delimiter;
            Prefix = prefix;
        }

        public QueryAttribute(string delimiter, string prefix, string format)
        {
            Delimiter = delimiter;
            Prefix = prefix;
            Format = format;
        }

        public QueryAttribute(CollectionFormat collectionFormat)
        {
            CollectionFormat = collectionFormat;
        }

        /// <summary>
        /// Used to customize the name of either the query parameter pair or of the form field when form encoding.
        /// </summary>
        /// <seealso cref="Prefix"/>
        public string Delimiter { get; protected set; } = ".";

        /// <summary>
        /// Used to customize the name of the encoded value.
        /// </summary>
        /// <remarks>
        /// Gets combined with <see cref="Delimiter"/> in the format <code>var name = $"{Prefix}{Delimiter}{originalFieldName}"</code>
        /// where <c>originalFieldName</c> is the name of the object property or method parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// class Form
        /// {
        ///   [Query("-", "dontlog")]
        ///   public string password { get; }
        /// }
        /// </code>
        /// will result in the encoded form having a field named <c>dontlog-password</c>.
        /// </example>
        public string? Prefix { get; protected set; }

        /// <summary>
        /// Used to customize the formatting of the encoded value.
        /// </summary>
        /// <example>
        /// <code>
        /// interface IServerApi
        /// {
        ///   [Get("/expenses")]
        ///   Task addExpense([Query(Format="0.00")] double expense);
        /// }
        /// </code>
        /// Calling <c>serverApi.addExpense(5)</c> will result in a URI of <c>{baseUri}/expenses?expense=5.00</c>.
        /// </example>
        public string? Format { get; set; }

        /// <summary>
        /// Specifies how the collection should be encoded.
        /// </summary>
        public CollectionFormat CollectionFormat
        {
            // Cannot make property nullable due to Attribute restrictions
            get => collectionFormat.GetValueOrDefault();
            set => collectionFormat = value;
        }

        public bool IsCollectionFormatSpecified => collectionFormat.HasValue;
    }

    [AttributeUsage(AttributeTargets.Method)]

    public class QueryUriFormatAttribute : RefitAttribute
    {
        public QueryUriFormatAttribute(UriFormat uriFormat)
        {
            UriFormat = uriFormat;
        }

        /// <summary>
        /// Specifies how the Query Params should be encoded.
        /// </summary>
        public UriFormat UriFormat { get; }
    }
}
