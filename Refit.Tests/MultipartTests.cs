﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Refit;
using System.Threading;
using Newtonsoft.Json;

namespace Refit.Tests
{
    public interface IRunscopeApi
    {
        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadStream(Stream stream);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadStreamPart(StreamPart stream);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadBytes(byte[] bytes);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadBytesPart([AliasAs("ByteArrayPartParamAlias")]ByteArrayPart bytes);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadString([AliasAs("SomeStringAlias")]string someString);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadFileInfo(IEnumerable<FileInfo> fileInfos, FileInfo anotherFile);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadFileInfoPart(IEnumerable<FileInfoPart> fileInfos, FileInfoPart anotherFile);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadJsonObject(ModelObject theObject);

        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadJsonObjects(IEnumerable<ModelObject> theObjects);


        [Multipart]
        [Post("/")]
        Task<HttpResponseMessage> UploadMixedObjects(IEnumerable<ModelObject> theObjects, AnotherModel anotherModel, FileInfo aFile, AnEnum anEnum, string aString, int anInt);

    }

    public class ModelObject
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
    }

    public class AnotherModel
    {
        public string[] Foos { get; set; }
    }

    public enum AnEnum
    {
        Val1,
        Val2
    }

    public class MultipartTests
    {
        const string BaseAddress = "https://api/";

        [Fact]
        public async Task MultipartUploadShouldWorkWithStream()
        {
            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Single(parts);

                    Assert.Equal("stream", parts[0].Headers.ContentDisposition.Name);
                    Assert.Equal("stream", parts[0].Headers.ContentDisposition.FileName);

                    using(var str = await parts[0].ReadAsStreamAsync())
                    using(var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            using (var stream = GetTestFileStream("Test Files/Test.pdf"))
            {
                var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                var result = await fixture.UploadStream(stream);
            }
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithByteArray()
        {
            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Single(parts);

                    Assert.Equal("bytes", parts[0].Headers.ContentDisposition.Name);
                    Assert.Equal("bytes", parts[0].Headers.ContentDisposition.FileName);
                    Assert.Null(parts[0].Headers.ContentType);
                    using (var str = await parts[0].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            using (var stream = GetTestFileStream("Test Files/Test.pdf"))
            using (var reader = new BinaryReader(stream))
            {
                var bytes = reader.ReadBytes((int)stream.Length);

                var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                var result = await fixture.UploadBytes(bytes);
            }
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithFileInfo()
        {
            var fileName = Path.GetTempFileName();
            var name = Path.GetFileName(fileName);

            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Equal(3, parts.Count);

                    Assert.Equal("fileInfos", parts[0].Headers.ContentDisposition.Name);
                    Assert.Equal(name, parts[0].Headers.ContentDisposition.FileName);
                    Assert.Null(parts[0].Headers.ContentType);
                    using (var str = await parts[0].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }
                    
                    Assert.Equal("fileInfos", parts[1].Headers.ContentDisposition.Name);
                    Assert.Equal(name, parts[1].Headers.ContentDisposition.FileName);
                    Assert.Null(parts[1].Headers.ContentType);
                    using (var str = await parts[1].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }

                    Assert.Equal("anotherFile", parts[2].Headers.ContentDisposition.Name);
                    Assert.Equal(name, parts[2].Headers.ContentDisposition.FileName);
                    Assert.Null(parts[2].Headers.ContentType);
                    using (var str = await parts[2].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }

                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };
            
            try
            {
                using (var stream = GetTestFileStream("Test Files/Test.pdf"))
                using (var outStream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                    outStream.Close();
                    
                    var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                    var result = await fixture.UploadFileInfo(new[] { new FileInfo(fileName), new FileInfo(fileName) }, new FileInfo(fileName));
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithString()
        {
            const string text = "This is random text";

            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Single(parts);

                    Assert.Equal("SomeStringAlias", parts[0].Headers.ContentDisposition.Name);
                    Assert.Null(parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("text/plain", parts[0].Headers.ContentType.MediaType);
                    Assert.Equal("utf-8", parts[0].Headers.ContentType.CharSet);
                    var str = await parts[0].ReadAsStringAsync();
                    Assert.Equal(text, str);
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            
            var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
            var result = await fixture.UploadString(text);
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithStreamPart()
        {
            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Single(parts);

                    Assert.Equal("stream", parts[0].Headers.ContentDisposition.Name);
                    Assert.Equal("test-streampart.pdf", parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/pdf", parts[0].Headers.ContentType.MediaType);

                    using (var str = await parts[0].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            using (var stream = GetTestFileStream("Test Files/Test.pdf"))
            {
                var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                var result = await fixture.UploadStreamPart(new StreamPart(stream, "test-streampart.pdf", "application/pdf"));
            }
        }


        [Fact]
        public async Task MultipartUploadShouldWorkWithByteArrayPart()
        {
            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Single(parts);

                    Assert.Equal("ByteArrayPartParamAlias", parts[0].Headers.ContentDisposition.Name);
                    Assert.Equal("test-bytearraypart.pdf", parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/pdf", parts[0].Headers.ContentType.MediaType);

                    using (var str = await parts[0].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            using (var stream = GetTestFileStream("Test Files/Test.pdf"))
            using (var reader = new BinaryReader(stream))
            {
                var bytes = reader.ReadBytes((int)stream.Length);

                var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                var result = await fixture.UploadBytesPart(new ByteArrayPart(bytes, "test-bytearraypart.pdf", "application/pdf"));
            }
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithFileInfoPart()
        {
            var fileName = Path.GetTempFileName();

            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Equal(3, parts.Count);

                    Assert.Equal("fileInfos", parts[0].Headers.ContentDisposition.Name);
                    Assert.Equal("test-fileinfopart.pdf", parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/pdf", parts[0].Headers.ContentType.MediaType);
                    using (var str = await parts[0].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }

                    Assert.Equal("fileInfos", parts[1].Headers.ContentDisposition.Name);
                    Assert.Equal("test-fileinfopart2.pdf", parts[1].Headers.ContentDisposition.FileName);
                    Assert.Null(parts[1].Headers.ContentType);
                    using (var str = await parts[1].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }

                    Assert.Equal("anotherFile", parts[2].Headers.ContentDisposition.Name);
                    Assert.Equal("additionalfile.pdf", parts[2].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/pdf", parts[2].Headers.ContentType.MediaType);
                    using (var str = await parts[2].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }

                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };


            try
            {
                using (var stream = GetTestFileStream("Test Files/Test.pdf"))
                using (var outStream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                    outStream.Close();

                    var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                    var result = await fixture.UploadFileInfoPart(new[] 
                    {
                        new FileInfoPart(new FileInfo(fileName), "test-fileinfopart.pdf", "application/pdf"),
                        new FileInfoPart(new FileInfo(fileName), "test-fileinfopart2.pdf", contentType: null)
                    }, new FileInfoPart(new FileInfo(fileName), fileName: "additionalfile.pdf", contentType: "application/pdf"));
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithAnObject()
        {
            var model1 = new ModelObject
            {
                Property1 = "M1.prop1",
                Property2 = "M1.prop2"
            };
            
            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Single(parts);

                    Assert.Equal("theObject", parts[0].Headers.ContentDisposition.Name);
                    Assert.Null(parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[0].Headers.ContentType.MediaType);
                    var result0 = JsonConvert.DeserializeObject<ModelObject>(await parts[0].ReadAsStringAsync());
                    Assert.Equal(model1.Property1, result0.Property1);
                    Assert.Equal(model1.Property2, result0.Property2);
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
            var result = await fixture.UploadJsonObject(model1);
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithObjects()
        {
            var model1 = new ModelObject
            {
                Property1 = "M1.prop1",
                Property2 = "M1.prop2"
            };

            var model2 = new ModelObject
            {
                Property1 = "M2.prop1"
            };


            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Equal(2, parts.Count);

                    Assert.Equal("theObjects", parts[0].Headers.ContentDisposition.Name);
                    Assert.Null(parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[0].Headers.ContentType.MediaType);
                    var result0 = JsonConvert.DeserializeObject<ModelObject>(await parts[0].ReadAsStringAsync());
                    Assert.Equal(model1.Property1, result0.Property1);
                    Assert.Equal(model1.Property2, result0.Property2);


                    Assert.Equal("theObjects", parts[1].Headers.ContentDisposition.Name);
                    Assert.Null(parts[1].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[1].Headers.ContentType.MediaType);
                    var result1 = JsonConvert.DeserializeObject<ModelObject>(await parts[1].ReadAsStringAsync());
                    Assert.Equal(model2.Property1, result1.Property1);
                    Assert.Equal(model2.Property2, result1.Property2);
                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
            var result = await fixture.UploadJsonObjects(new[] { model1, model2 });
        }

        [Fact]
        public async Task MultipartUploadShouldWorkWithMixedTypes()
        {
            var fileName = Path.GetTempFileName();
            var name = Path.GetFileName(fileName);

            var model1 = new ModelObject
            {
                Property1 = "M1.prop1",
                Property2 = "M1.prop2"
            };

            var model2 = new ModelObject
            {
                Property1 = "M2.prop1"
            };

            var anotherModel = new AnotherModel
            {
                Foos = new []{"bar1", "bar2"}
            };

            var handler = new MockHttpMessageHandler
            {
                Asserts = async content =>
                {
                    var parts = content.ToList();

                    Assert.Equal(7, parts.Count);

                    Assert.Equal("theObjects", parts[0].Headers.ContentDisposition.Name);
                    Assert.Null(parts[0].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[0].Headers.ContentType.MediaType);
                    var result0 = JsonConvert.DeserializeObject<ModelObject>(await parts[0].ReadAsStringAsync());
                    Assert.Equal(model1.Property1, result0.Property1);
                    Assert.Equal(model1.Property2, result0.Property2);


                    Assert.Equal("theObjects", parts[1].Headers.ContentDisposition.Name);
                    Assert.Null(parts[1].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[1].Headers.ContentType.MediaType);
                    var result1 = JsonConvert.DeserializeObject<ModelObject>(await parts[1].ReadAsStringAsync());
                    Assert.Equal(model2.Property1, result1.Property1);
                    Assert.Equal(model2.Property2, result1.Property2);

                    Assert.Equal("anotherModel", parts[2].Headers.ContentDisposition.Name);
                    Assert.Null(parts[2].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[2].Headers.ContentType.MediaType);
                    var result2 = JsonConvert.DeserializeObject<AnotherModel>(await parts[2].ReadAsStringAsync());
                    Assert.Equal(2, result2.Foos.Length);
                    Assert.Equal("bar1", result2.Foos[0]);
                    Assert.Equal("bar2", result2.Foos[1]);


                    Assert.Equal("aFile", parts[3].Headers.ContentDisposition.Name);
                    Assert.Equal(name, parts[3].Headers.ContentDisposition.FileName);
                    Assert.Null(parts[3].Headers.ContentType);
                    using (var str = await parts[3].ReadAsStreamAsync())
                    using (var src = GetTestFileStream("Test Files/Test.pdf"))
                    {
                        Assert.True(StreamsEqual(src, str));
                    }

                    Assert.Equal("anEnum", parts[4].Headers.ContentDisposition.Name);
                    Assert.Null(parts[4].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[4].Headers.ContentType.MediaType);
                    var result4 = JsonConvert.DeserializeObject<AnEnum>(await parts[4].ReadAsStringAsync());
                    Assert.Equal(AnEnum.Val2, result4);

                    Assert.Equal("aString", parts[5].Headers.ContentDisposition.Name);
                    Assert.Null(parts[5].Headers.ContentDisposition.FileName);
                    Assert.Equal("text/plain", parts[5].Headers.ContentType.MediaType);
                    Assert.Equal("utf-8", parts[5].Headers.ContentType.CharSet);
                    Assert.Equal("frob", await parts[5].ReadAsStringAsync());

                    Assert.Equal("anInt", parts[6].Headers.ContentDisposition.Name);
                    Assert.Null(parts[6].Headers.ContentDisposition.FileName);
                    Assert.Equal("application/json", parts[6].Headers.ContentType.MediaType);
                    var result6 = JsonConvert.DeserializeObject<int>(await parts[6].ReadAsStringAsync());
                    Assert.Equal(42, result6);

                }
            };

            var settings = new RefitSettings()
            {
                HttpMessageHandlerFactory = () => handler
            };

            try
            {
                using (var stream = GetTestFileStream("Test Files/Test.pdf"))
                using (var outStream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                    outStream.Close();

                    var fixture = RestService.For<IRunscopeApi>(BaseAddress, settings);
                    var result = await fixture.UploadMixedObjects(new[] { model1, model2 }, anotherModel, new FileInfo(fileName), AnEnum.Val2, "frob", 42);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Fact]
        public void MultiPartConstructorShouldThrowArgumentNullExceptionWhenNoFileName()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var byteArrayPart = new ByteArrayPart(new byte[0], null, "application/pdf");
            });
        }

        [Fact]
        public void FileInfoPartConstructorShouldThrowArgumentNullExceptionWhenNoFileInfo()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var fileInfoPart = new FileInfoPart(null, "file.pdf", "application/pdf");
            });
        }

        private static Stream GetTestFileStream(string relativeFilePath)
        {
            const char namespaceSeparator = '.';

            // get calling assembly
            var assembly = Assembly.GetCallingAssembly();

            // compute resource name suffix
            var relativeName = "." + relativeFilePath
                .Replace('\\', namespaceSeparator)
                .Replace('/', namespaceSeparator)
                .Replace(' ', '_');

            // get resource stream
            var fullName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(relativeName, StringComparison.InvariantCulture));
            if (fullName == null) {
                throw new Exception($"Unable to find resource for path \"{relativeFilePath}\". Resource with name ending on \"{relativeName}\" was not found in assembly.");
            }

            var stream = assembly.GetManifestResourceStream(fullName);
            if (stream == null) {
                throw new Exception($"Unable to find resource for path \"{relativeFilePath}\". Resource named \"{fullName}\" was not found in assembly.");
            }

            return stream;
        }

        bool StreamsEqual(Stream a, Stream b)
        {
            if (a == null &&
                b == null)
                return true;
            if (a == null ||
                b == null)
            {
                throw new ArgumentNullException(
                    a == null ? "a" : "b");
            }
            
            if (a.Length < b.Length)
                return false;
            if (a.Length > b.Length)
                return false;

            for (var i = 0; i < a.Length; i++)
            {
                var aByte = a.ReadByte();
                var bByte = b.ReadByte();
                if (aByte != bByte)
                    return false;
            }

            return true;
        }

        class MockHttpMessageHandler : HttpMessageHandler
        {
            public Func<MultipartFormDataContent, Task> Asserts { get; set; }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var content = request.Content as MultipartFormDataContent;
                Assert.IsType<MultipartFormDataContent>(content);
                Assert.NotNull(Asserts);

                await Asserts(content);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
    }
}
