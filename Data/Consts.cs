using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public static class Consts
    {
        public const string MetacriticUrl = "https://www.metacritic.com";

        public const string PageBaseUrl = MetacriticUrl + "/browse/movies/genre/date/";

        public static IReadOnlyList<string> Genres = new List<string>()
        {
            "Action",
            "Adventure",
            "Animation",
            "Biography",
            "Comedy",
            "Crime",
            "Documentary",
            "Drama",
            "Family",
            "Fantasy",
            "Film-Noir",
            "History",
            "Horror",
            "Music",
            "Musical",
            "Mystery",
            "News",
            "Reality-TV",
            "Romance",
            "Sci-Fi",
            "Short",
            "Sport",
            "Thriller",
            "War",
            "Western"
        };
    }
}
