using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Refit.Extensions.Properties
{
    public class PropertyProviderFactory
    {
        private readonly List<PropertyProvider> propertyProviders = new();

        /// <summary>
        /// Populates any custom <see cref="Attribute"/> present on the Refit interface and/or the currently executing method on the Refit interface that is not a subclass of <see cref="RefitAttribute"/>
        /// into the <see cref="HttpRequestMessage"/> properties with the key as the Name property on the <see cref="Type"/> of the <see cref="Attribute"/>.
        /// When the same attribute is present on both the Refit interface and the interface method, the one on the method takes precedence.
        /// </summary>
        public PropertyProviderFactory CustomAttributePropertyProvider()
        {
            return PropertyProvider(new CustomAttributePropertyProvider());
        }

        /// <summary>
        /// Populates the <see cref="MethodInfo"/> of the currently executing method on the Refit interface into the <see cref="HttpRequestMessage"/> properties
        /// </summary>
        public PropertyProviderFactory MethodInfoPropertyProvider()
        {
            return PropertyProvider(new MethodInfoPropertyProvider());
        }

        /// <summary>
        /// Populates the Refit interface type into the <see cref="HttpRequestMessage"/> properties
        /// </summary>
        public PropertyProviderFactory RefitTargetInterfaceTypePropertyProvider()
        {
            return PropertyProvider(new RefitTargetInterfaceTypePropertyProvider());
        }

        /// <summary>
        /// Add a custom <see cref="PropertyProvider"/>
        /// </summary>
        public PropertyProviderFactory PropertyProvider(PropertyProvider propertyProvider)
        {
            if (propertyProvider == null)
                throw new ArgumentNullException(nameof(propertyProvider));

            propertyProviders.Add(propertyProvider);

            return this;
        }

        public List<PropertyProvider> Build()
        {
            return propertyProviders;
        }

        /// <summary>
        /// This allows you to build up a list of <see cref="PropertyProvider"/> implementations that will populate data into the <see cref="HttpRequestMessage"/> properties for use inside <see cref="HttpClient"/> middleware.
        /// </summary>
        public static PropertyProviderFactory WithPropertyProviders()
        {
            return new PropertyProviderFactory();
        }

        /// <summary>
        /// By default if no <see cref="PropertyProvider"/> implementations are configured a <see cref="RefitTargetInterfaceTypePropertyProvider"/> will be used.
        /// </summary>
        public static List<PropertyProvider> WithDefaultPropertyProviders()
        {
            return new List<PropertyProvider> { new RefitTargetInterfaceTypePropertyProvider() };
        }

        /// <summary>
        /// This can be used to override the default behavior if you don't want any property providers.
        /// </summary>
        public static List<PropertyProvider> WithoutPropertyProviders()
        {
            return new List<PropertyProvider>();
        }
    }
}
