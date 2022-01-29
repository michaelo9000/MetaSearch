using Data.Models;
using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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

        public async Task AddMovie(Movie movieData)
        {
            var movieRef = _movies.Document(movieData.UrlName);

            try
            {
                await movieRef.CreateAsync(movieData);
            }
            catch (Grpc.Core.RpcException e)
            {
                if (e.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
                {
                    await movieRef.SetAsync(movieData);
                }
                else
                {
                    Console.WriteLine
                }
            }
        }

        public async Task<List<Movie>> GetMovies
            (string genreOne, string genreTwo, int scoreLower, int scoreUpper, int yearLower, int yearUpper)
        {
            // Kick off the query with the param that will exclude the most data in the initial retrieval.
            var query = InitQueryWithPrimaryFilter();

            // Also use the first genre, if one is selected.
            if (!string.IsNullOrWhiteSpace(genreOne))
                query = query.WhereArrayContains("Genres", genreOne);

            // TODO USE A 'TAKE' TYPE FUNCTION TO LIMIT RESULTS, AND THEN PAGINATE
            var snapshot = await query.GetSnapshotAsync();

            var movies = snapshot.Documents.Select(i => i.ConvertTo<Movie>());

            if (!string.IsNullOrWhiteSpace(genreTwo))
                movies = movies.Where(i => i.Genres.Contains(genreTwo));

            movies = movies
                .Where(i => i.Score >= scoreLower && i.Score <= scoreUpper)
                .Where(i => i.Year >= yearLower && i.Year <= yearUpper)
                .OrderByDescending(i => i.Score);

            return movies.ToList();

            Query InitQueryWithPrimaryFilter()
            {
                if (scoreLower >= 70)
                    return _movies.WhereGreaterThanOrEqualTo("Score", scoreLower);
                if (yearLower >= 2000)
                    return _movies.WhereGreaterThanOrEqualTo("Year", yearLower);
                if (scoreUpper < 70)
                    return _movies.WhereLessThanOrEqualTo("Score", scoreUpper);
                
                return _movies.WhereLessThanOrEqualTo("Year", yearUpper);
            }
        }
    }
}
