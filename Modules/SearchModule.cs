using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspectify.Modules
{
    /// <summary>
    /// Abstract representation of a search module.
    /// </summary>
    public abstract class SearchModule
    {
        /// <summary>
        /// Gets the name of the search module.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the description of the search module.
        /// </summary>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Gets the maximum amount of results.
        /// </summary>
        public abstract int MaxResults
        {
            get;
        }

        /// <summary>
        /// Gets or sets a <see cref="SearchModuleResult"/> collection.
        /// </summary>
        public abstract List<SearchModuleResult> Results
        {
            get;
            set;
        }

        /// <summary>
        /// Performs the action to retrieve a <see cref="SearchModuleResult"/> collection.
        /// </summary>
        /// <param name="query">The query to use for retrieving results.</param>
        public abstract Task RetrieveResultsAsync(string query);
    }
}
