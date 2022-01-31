using Core;
using Data;
using Data.Extensions;
using Data.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Extensions;

namespace WebScraper
{
    public static class WebDataProcessor
    {
        public static HtmlWeb web = new HtmlWeb();

        public static IEnumerable<string> GetMoviePathList(int pageNumber, string genre)
        {
            // Pages are zero-based in the query string, but not in the UI.
            HtmlNode movieListPage = web.Load($"{Consts.PageBaseUrl}{genre}?page={pageNumber - 1}").DocumentNode;

            var movieLinkNodes = movieListPage
                .SelectNodes("//a[@class='title']");

            var moviePaths = movieLinkNodes?
                .SelectMany(i => i.Attributes)
                .Where(i => i.Name == "href")
                .Select(i => i.Value);

            return moviePaths;
        }

        public static async Task<HtmlNode> GetMoviePageDataAsync(string path)
        {
            var url = $"{Consts.MetacriticUrl}{path}/details";
            var moviePageRequest = await web.LoadFromWebAsync(url);
            var moviePage = moviePageRequest.DocumentNode;

            if (!moviePage.HasChildNodes)
            {
                Logger.Log("Received malformed document data. Waiting 15 seconds and trying again.");
                Thread.Sleep(15000);
                moviePageRequest = await web.LoadFromWebAsync(url);
                moviePage = moviePageRequest.DocumentNode;
            }

            return moviePage;
        }

        public static Movie GetMovieFromPageData(HtmlNode moviePage, string path)
        {
            var movieData = new Movie();
            movieData.UrlName = path.Split('/')[2];

            movieData.Score = Convert.ToInt32(moviePage.SelectNodeText("span", "metascore_w"));

            var titleSection = moviePage.SelectSingleNodeContains("div", "product_page_title");
            movieData.Name = titleSection.SelectSingleNode("//h1").InnerHtml;

            var releaseDateArray = titleSection
                .SelectSingleNode("//span[@class='release_date']")
                .Elements("span")
                .FirstOrDefault(i => !i.Attributes.Any(a => a.Name == "class"))
                .InnerText
                .Split(',');

            movieData.Year = releaseDateArray.Length > 1 ?
                Convert.ToInt32(releaseDateArray[1])
                :
                DateTime.Today.Year + 1;

            var detailsTable = moviePage.SelectSingleNode("//table[@class='details']").FirstChild;
            movieData.Genres = detailsTable.GetDataArrayFromDetailsTable("genres");
            movieData.Languages = detailsTable.GetDataArrayFromDetailsTable("languages");
            movieData.Runtime = detailsTable
                .GetDataFromDetailsTable("runtime")
                ?.TrimToIntAndConvert();

            var creditsSection = moviePage
                .SelectSingleNode("//div[@class='credits_list']");

            var directorElements = creditsSection.GetCreditElements("Director");
            movieData.DirectorNames = directorElements?.Select(i => i.InnerText.Trim().Replace("\\n", "")).ToArray();
            movieData.DirectorUrlNames = directorElements?.GetCreditUrls();

            var writerElements = creditsSection.GetCreditElements("Writer");
            movieData.WriterNames = writerElements?.Select(i => i.InnerText.Trim().Replace("\\n", "")).ToArray();
            movieData.WriterUrlNames = writerElements?.GetCreditUrls();

            var castElements = creditsSection.GetCreditElements("Principal Cast");
            movieData.CastNames = castElements?.Select(i => i.InnerText.Trim().Replace("\\n", "")).ToArray();
            movieData.CastUrlNames = castElements?.GetCreditUrls();

            return movieData;
        }
    }
}
