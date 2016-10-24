using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Inspectify
{
    public class HttpUtility
    {
        #region General

        #region Constants
        #endregion

        private static readonly Lazy<HttpUtility> current = new Lazy<HttpUtility>(() => new HttpUtility());

        /// <summary>
        /// Gets the current instance of <see cref="HttpUtility"/>.
        /// </summary>
        public static HttpUtility Current { get { return current.Value; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        private HttpUtility()
        {
        }
        #endregion

        /// <summary>
        /// Sends a GET request to the provided url.
        /// </summary>
        /// <param name="url">The url to GET the contents from.</param>
        /// <returns>The response's content, or an exception if an error occurred.</returns>
        public async Task<string> Get(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"{response.StatusCode} '{url}'");
                }
            }
        }
    }
}
