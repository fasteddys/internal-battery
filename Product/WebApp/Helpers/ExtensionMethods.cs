using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UpDiddy.Helpers
{
    public static class ExtensionMethods
    {

        public static string PartialToHtmlMarkup<TModel>(this IHtmlHelper helper, string PartialPath, TModel model)
        {
            var content = helper.Partial(PartialPath, model);
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}