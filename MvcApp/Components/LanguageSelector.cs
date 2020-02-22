using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace MvcApp.Components
{
    /// <summary>
    /// Language selector view component
    /// </summary>
    public class LanguageSelector : ViewComponent
    {
        /// <summary>
        /// Invokes the component and returns a view
        /// </summary>
        public IViewComponentResult Invoke()
        {
            var List = Languages.Items;
            return View(List);
        }
    }
}
