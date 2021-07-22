using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refit
{
    /// <summary>
    /// Contains Refit-defined properties on the HttpRequestMessage.Properties/Options
    /// </summary>
    public static class HttpRequestMessageOptions
    {
        /// <summary>
        /// Returns the <see cref="System.Type"/> of the top-level interface where the method was called from
        /// </summary>
        public static string InterfaceType { get; } = "Refit.InterfaceType";
        public static string MethodInfo { get; } = "Refit.MethodInfo";

        public static string PropertyProviderException { get; } = "Refit.PropertyProviderException";
    }
}
