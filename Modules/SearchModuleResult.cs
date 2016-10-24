using System;
using System.Windows.Media;

namespace Inspectify.Modules
{
    /// <summary>
    /// Describes a result for a search module.
    /// </summary>
    public class SearchModuleResult
    {
        public SearchModuleResult(ImageSource icon, string description, Action action)
        {
            this.Icon = icon;
            this.Description = description;
            this.Action = action;
        }

        /// <summary>
        /// Gets or sets the icon of the search result.
        /// </summary>
        public ImageSource Icon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the search result, that will be shown to the user.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the action to perform.
        /// </summary>
        public Action Action
        {
            get;
            set;
        }
    }
}