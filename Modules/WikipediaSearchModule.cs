using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Inspectify.Modules
{
    public class WikipediaSearchModule : SearchModule, IWebSearchModule
    {
        #region General
        public override string Name
        {
            get
            {
                return "Wikipedia";
            }
        }

        public override string Description
        {
            get
            {
                return "Search module that allows searching on Wikipedia.";
            }
        }

        public override int MaxResults
        {
            get
            {
                return 1;
            }
        }

        public override List<SearchModuleResult> Results
        {
            get;
            set;
        }
        
        public string Url
        {
            get
            {
                return "https://en.wikipedia.org/w/api.php?action=opensearch&search={0}&limit=1&namespace=0&redirects=resolve&format=json";
            }
        }

        private ImageSource icon;
        /// <summary>
        /// Gets Wikipedia's icon.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                if (icon == null)
                {
                    this.icon = Utility.Current.GetWpfImageSource("/Resources/wikipedia_32.png");
                }

                return this.icon;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Performs the action to retrieve a <see cref="SearchModuleResult"/> collection.
        /// </summary>
        /// <param name="query">The query to use for retrieving results.</param>
        public override async Task RetrieveResultsAsync(string query)
        {
            string url = string.Format(this.Url, query);

            var jsonResult = await HttpUtility.Current.Get(url);

            JArray result = JsonConvert.DeserializeObject<JArray>(jsonResult);

            if (result != null && result.Count == 4)
            {
                this.Results = new List<SearchModuleResult>();

                string[] searchResults = result[1].Select(j => j.Value<string>()).ToArray();
                string[] searchDescriptions = result[2].Select(j => j.Value<string>()).ToArray();
                string[] searchUrls = result[3].Select(j => j.Value<string>()).ToArray();

                for (int i = 0; i < this.MaxResults; i++)
                {
                    string description = $"{searchResults[i]}{Environment.NewLine}{searchDescriptions[i]}";
                    string retrievedUrl = searchUrls[i];

                    this.Results.Add(new SearchModuleResult(this.Icon, description, () => Utility.Current.OpenFile(retrievedUrl)));
                }
            }
            else
            {
                this.Results = null;
            }
        }
        #endregion
    }
}
