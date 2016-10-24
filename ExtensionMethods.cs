using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspectify
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        /// <param name="text">The value to extend on.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="stringComparison">The culture, case, and sort rules to be used.</param>
        /// <returns>True if the value parameter occurs within this string, or if value is empty string (""); otherwise, false.</returns>
        public static bool Contains(this string text, string value, StringComparison stringComparison)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }

}
