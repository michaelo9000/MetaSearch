using Data.Models;
using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Core;
using Data.Queries;

namespace Data
{
    public class DatabaseClient
    {
        private FirestoreDb _db;
        private CollectionReference _movies;
        

        public DatabaseClient()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "metasearch-1e069-688dc9635c89.json");
            _db = FirestoreDb.Create("metasearch-1e069");
            _movies = _db.Collection("movies");
        }

        // Returns whether or not the movie was new to the database, or if the data was replaced.
        public async Task<bool> AddMovie(Movie movieData)
        {
            var movieRef = _movies.Document(movieData.UrlName);

            try
            {
                await movieRef.CreateAsync(movieData);
                return false;
            }
            catch (Grpc.Core.RpcException e)
            {
                if (e.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
                {
                    Logger.Log($"Data exists at {movieData.UrlName} - replacing.");
                    await movieRef.SetAsync(movieData);
                    return true;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<List<Movie>> GetMovies(MovieQuery querySettings)
        {
            // Kick off the query with the param that will exclude the most data in the initial retrieval.
            var query = InitQueryWithPrimaryFilter();

            // Also use the first genre, if one is selected.
            if (!string.IsNullOrWhiteSpace(querySettings.GenreOne))
                query = query.WhereArrayContains("Genres", querySettings.GenreOne);

            var snapshot = await query.GetSnapshotAsync();

            var movies = snapshot.Documents.Select(i => i.ConvertTo<Movie>());

            if (!string.IsNullOrWhiteSpace(querySettings.GenreTwo))
                movies = movies.Where(i => i.Genres.Contains(querySettings.GenreTwo));

            movies = movies
                .Where(i => i.Score >= querySettings.ScoreMin && i.Score <= querySettings.ScoreMax)
                .Where(i => i.Year >= querySettings.YearMin&& i.Year <= querySettings.YearMax)
                .OrderByDescending(i => i.Score)
                // Paginating. Firestore doesn't support bounding results by index which kinda sucks??
                // TODO Need to deliver the full results to the model and then paginate.
                .Skip(querySettings.PageNumber * querySettings.PageResultCount)
                .Take(querySettings.PageResultCount);

            return movies.ToList();

            Query InitQueryWithPrimaryFilter()
            {
                if (querySettings.ScoreMin >= 70)
                    return _movies.WhereGreaterThanOrEqualTo("Score", querySettings.ScoreMin);
                if (querySettings.YearMin >= 2000)
                    return _movies.WhereGreaterThanOrEqualTo("Year", querySettings.YearMin);
                if (querySettings.ScoreMax < 70)
                    return _movies.WhereLessThanOrEqualTo("Score", querySettings.ScoreMax);
                
                return _movies.WhereLessThanOrEqualTo("Year", querySettings.YearMax);
            }
        }
    }
}
