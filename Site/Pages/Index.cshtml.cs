using Data;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Queries;

namespace Site.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DatabaseClient _dbClient;

        private const int _pageResultCount = 50;

        // Display properties
        public List<Movie> Movies { get; set; }
        public SelectList Genres { get; set; }

        // Search properties
        [BindProperty(SupportsGet = true)]
        public string GenreOne { get; set; }
        [BindProperty(SupportsGet = true)]
        public string GenreTwo { get; set; }
        [BindProperty(SupportsGet = true)]
        public int ScoreLower { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public int ScoreUpper { get; set; } = 100;
        [BindProperty(SupportsGet = true)]
        public int YearLower { get; set; } = 1900;
        [BindProperty(SupportsGet = true)]
        public int YearUpper { get; set; } = 2021;

        public IndexModel()
        {
            _dbClient = new DatabaseClient();

            Genres = new SelectList(Consts.Genres);
        }

        public async Task OnGetAsync()
        {
            if (YearUpper == 0)
                YearUpper = DateTime.Today.Year;
            if (ScoreUpper == 0)
                ScoreUpper = 100;

            var query = new MovieQuery(GenreOne, GenreTwo, ScoreLower, ScoreUpper, YearLower, YearUpper, 1, _pageResultCount);

            Movies = await _dbClient.GetMovies(query);
        }
    }
}
