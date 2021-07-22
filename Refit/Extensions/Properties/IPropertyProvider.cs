using System;
using System.Collections.Generic;
using System.Reflection;

namespace Refit.Extensions.Properties
{
    public interface PropertyProvider
    {
        void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType);
    }
}
