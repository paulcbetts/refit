using System;
using System.Collections.Generic;
using System.Reflection;

namespace Refit.Extensions.Properties
{
    public class RefitTargetInterfaceTypePropertyProvider : PropertyProvider
    {
        public void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType)
        {
            properties[HttpRequestMessageOptions.InterfaceType] = refitTargetInterfaceType;
        }
    }
}
