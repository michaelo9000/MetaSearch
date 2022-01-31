using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Queries
{
    public class MovieQuery
    {
        public MovieQuery(string genreOne, string genreTwo, int scoreLower, int scoreUpper, int yearLower, int yearUpper, int startAt, int resultsPerPage)
        {
            GenreOne = genreOne;
            GenreTwo = genreTwo;
            ScoreMin = scoreLower;
            ScoreMax = scoreUpper;
            YearMin = yearLower;
            YearMax = yearUpper;
            PageNumber = startAt;
            PageResultCount = resultsPerPage;
        }

        public string GenreOne { get; set; }
        public string GenreTwo { get; set; }
        public int ScoreMin { get; set; }
        public int ScoreMax { get; set; }
        public int YearMin { get; set; }
        public int YearMax { get; set; }
        public int PageNumber { get; set; }
        public int PageResultCount { get; set; }
    }
}
