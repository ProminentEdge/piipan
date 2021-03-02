﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public PiiRecord Query { get; set; }

        private readonly OrchestratorApiRequest _apiRequest = new OrchestratorApiRequest();
        public List<PiiRecord> QueryResult { get; private set; } = new List<PiiRecord>();
        public String RequestError { get; private set; }
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync(PiiRecord query)
        {
            if (ModelState.IsValid)
            {
                QueryResult = await _apiRequest.SendQuery(
                    Environment.GetEnvironmentVariable("OrchApiUri"),
                    query
                );

                try
                {
                    NoResults = QueryResult.Count == 0;
                    Title = "NAC Query Results";
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    RequestError = "There was an error running your search";
                }
            }
            return Page();
        }

        public string Title { get; private set; } = "";

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
