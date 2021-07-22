using System;
using System.Collections.Generic;
using System.Reflection;

namespace Refit.Extensions.Properties
{
    public class CustomAttributePropertyProvider : PropertyProvider
    {
        public void ProvideProperties(IDictionary<string, object?> properties, MethodInfo methodInfo, Type refitTargetInterfaceType)
        {
            foreach (var interfaceAttribute in refitTargetInterfaceType.GetCustomAttributes())
            {
                if (interfaceAttribute is RefitAttribute)
                {
                    continue;
                }

                properties[interfaceAttribute.GetType().Name] = interfaceAttribute;
            }

            foreach (var methodAttribute in methodInfo.GetCustomAttributes())
            {
                if (methodAttribute is RefitAttribute)
                {
                    continue;
                }

                properties[methodAttribute.GetType().Name] = methodAttribute;
            }
        }
    }
}
