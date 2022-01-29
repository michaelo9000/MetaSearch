using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace WebScraper.Extensions
{
    static class HtmlNodeExtensions
    {
        public static HtmlNode SelectSingleNodeContains(this HtmlNode parentNode, string element, string className)
        {
            return parentNode.SelectSingleNode($"//{element}[contains(@class, '{className}')]");
        }
        public static HtmlNode SelectSingleNodeContains(this HtmlNode parentNode, string element, string attributeName, string attributeValue)
        {
            return parentNode.SelectSingleNode($"//{element}[contains(@{attributeName}, '{attributeValue}')]");
        }

        public static string SelectNodeText(this HtmlNode parentNode, string element, string className)
        {
            return parentNode.SelectSingleNodeContains(element, className).InnerHtml;
        }

        public static string GetDataFromDetailsTable(this HtmlNode parentNode, string rowClassName)
        {
            return parentNode
                ?.SelectSingleNode($"//tr[@class='{rowClassName}']")
                ?.Elements("td")
                ?.FirstOrDefault(i => i.Attributes.Any(a => a.Value == "data"))
                .InnerHtml;
        }

        public static string[] GetDataArrayFromDetailsTable(this HtmlNode parentNode, string rowClassName)
        {
            return parentNode
                ?.SelectSingleNode($"//tr[@class='{rowClassName}']")
                ?.Elements("td")
                ?.FirstOrDefault(i => i.Attributes.Any(a => a.Value == "data"))
                ?.Elements("span")
                ?.Select(i => i.InnerHtml)
                .ToArray();
        }

        public static IEnumerable<HtmlNode> GetCreditElements(this HtmlNode creditsList, string creditType)
        {
            return creditsList
                ?.SelectSingleNodeContains("table", "summary", creditType)
                ?.Descendants("a");
        }

        public static string[] GetCreditUrls(this IEnumerable<HtmlNode> creditElements)
        {
            return creditElements
                .SelectMany(i => i.Attributes
                    .Where(a => a.Name == "href")
                    .Select(a => a.Value
                        .Replace("/person/", "")
                        .Replace("?filter-options=movies", "")
                    )
                )
                .ToArray();
        }
    }
}
