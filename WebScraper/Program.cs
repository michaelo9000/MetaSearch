using Core;
using Data;
using Data.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.Extensions;

namespace WebScraper
{
    class Program
    {
        const int _sleepTime = 20;

        // CONFIG FOR INCREMENTAL SCRAPING
        // Keep these numbers set to grab the next movie that needs storing.
        // Both numbers are one-based, so they should never be zero.
        const int _skipTo = 1;
        const int _pageNumber = 1;

        const bool _useGenreCompleteThreshold = false;
        const int _genreCompleteThreshold = 5;

        static string _currentGenre;

        static string[] _completedGenres = new string[] { "Action" };

        static async Task Main(string[] args)
        {
            DatabaseClient dbClient = new DatabaseClient();

            ThreadPool.QueueUserWorkItem(new WaitCallback((object index) => WriteGenre()));
            Console.ReadLine();
            
            foreach (var genreName in Consts.Genres)
            {
                if (_completedGenres.Contains(genreName))
                {
                    Logger.Log($"Already completed {genreName} genre apparently. Skipping!");
                }

                _currentGenre = genreName;

                Logger.Log($"Beginning scraping {genreName} movies, newest first.");

                var alreadyStoredCount = 0;
                var pageCount = _pageNumber;

                // Need this over simply breaking because I need to be able to break the while from within the foreach.
                var keepWhilin = true;
                while (keepWhilin)
                {
                    var pageStartTime = DateTime.Now;

                    var moviePaths = WebDataProcessor.GetMoviePathList(pageCount, genreName);

                    if (moviePaths == null || !moviePaths.Any())
                    {
                        Logger.Log($"No movie links found on page {pageCount}! Either this is the last page (yay, change genres), or something went wrong.");
                        break;
                    }

                    if (_skipTo > 1)
                        moviePaths = moviePaths.Skip(_skipTo - 1);

                    var movieCount = _skipTo;

                    foreach (var path in moviePaths)
                    {
                        var startTime = DateTime.Now;

                        try
                        {
                            var moviePage = await WebDataProcessor.GetMoviePageDataAsync(path);
                            var movieData = WebDataProcessor.GetMovieFromPageData(moviePage, path);
                            var alreadyStored = await dbClient.AddMovie(movieData);
                            Logger.Log($"{pageCount.ToString("D2")} - {movieCount.ToString("D2")}: {movieData.Name}.");

                            if (alreadyStored) alreadyStoredCount++;
                            if (_useGenreCompleteThreshold && alreadyStoredCount > _genreCompleteThreshold)
                            {
                                // TODO this might not work due to genre crossover - i.e. Adventure movies are usually also Action movies, so after storing the new Action movies none of the Adventure movies will be checked.
                                // Need to supplement with a createddate on records and createddate must be in the past before the movie is considered 'already stored'?
                                Logger.Log($"Encountered {alreadyStoredCount} {genreName} movies that have been stored before. I have been configured to believe that this is the end of the new movies for this genre, so on to the next!");
                                keepWhilin = false;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Failed on page {pageCount} (one-based), movie {movieCount}");
                            Logger.Log($"Exception: {e.Message}");
                            Logger.Log($"Stacktrace: {e.StackTrace}");
                            keepWhilin = false;
                            break;
                        }

                        Logger.LogTimeDiff(startTime);
                        movieCount++;
                    }

                    Logger.Log($"Completed page {pageCount}.");
                    Logger.LogTimeDiff(pageStartTime);

                    Logger.Log($"Sleeping for {_sleepTime} seconds to give metacritic a break.");
                    Thread.Sleep(_sleepTime * 1000);

                    pageCount++;
                }
            }

            Console.WriteLine("END");
            Console.ReadLine();
        }
        
        public static async void WriteGenre()
        {
            // Never stop writing this.
            while (Logger.LastLine != $"NOTICE: Searching {_currentGenre} movies!")
            {
                Logger.Log($"NOTICE: Searching {_currentGenre} movies!");
                Thread.Sleep(60000);
            }
            Logger.Log("NOTICES ended.");
        }
    }
}
