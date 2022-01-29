using Data;
using Data.Models;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Extensions;

namespace WebScraper
{
    class Program
    {
        const string metacriticUrl = "https://www.metacritic.com";
        const string pageBaseUrl = metacriticUrl + "/browse/movies/score/metascore/all/filtered?page=";

        // CONFIG FOR INCREMENTAL SCRAPING
        // Keep these numbers set to grab the next movie that needs storing.
        const int _skipTo = 1;
        const int _pageNumber = 65;

        static async Task Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb();
            DatabaseClient dbClient = new DatabaseClient();

            var pageCount = _pageNumber;

            while (pageCount < 146)
            {
                // Pages are zero-based in the query string, but not in the UI. 
                HtmlNode movieListPage = web.Load($"{pageBaseUrl}{pageCount - 1}").DocumentNode;

                var movieLinkNodes = movieListPage
                    .SelectNodes("//a[@class='title']");

                var moviePaths = movieLinkNodes?
                    .SelectMany(i => i.Attributes)
                    .Where(i => i.Name == "href")
                    .Select(i => i.Value);

                var pageStartTime = DateTime.Now;

                if (_skipTo > 1)
                    moviePaths = moviePaths.Skip(_skipTo - 1);

                var movieCount = _skipTo;

                foreach (var path in moviePaths)
                {
                    var startTime = DateTime.Now;

                    var movieData = new Movie();
                    movieData.UrlName = path.Split('/')[2];

                    var url = $"{metacriticUrl}{path}/details";
                    var moviePageRequest = await web.LoadFromWebAsync(url);
                    var moviePage = moviePageRequest.DocumentNode;

                    if (!moviePage.HasChildNodes)
                    {
                        Log("Received malformed document data. Waiting 15 seconds and trying again.");
                        Thread.Sleep(15000);
                        moviePageRequest = await web.LoadFromWebAsync(url);
                        moviePage = moviePageRequest.DocumentNode;
                    }

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

                    await dbClient.AddMovie(movieData);

                    Log($"Stored #{movieCount.ToString("D2")}: {movieData.Name}.");
                    LogTimeDiff(startTime);

                    movieCount++;
                }

                Log($"Completed page {pageCount}.");
                LogTimeDiff(pageStartTime);

                Log("Sleeping for 20 seconds to give metacritic a break.");
                Thread.Sleep(20000);

                pageCount++;
            }

            Console.ReadLine();
        }

        public static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {message}");
        }

        public static void LogTimeDiff(DateTime time)
        {
            Log($"Took {Math.Round((DateTime.Now - time).TotalMilliseconds / 1000, 2)} seconds.");
        }
    }
}
