using alfariq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class CompletedSessionAjaxViewModel
    {
        public CompletedTrialBlockAjaxViewModel[] TrialBlocks { get; set; }
        public int SessionId { get; set; }
        public string Username { get; set; }
    }

    public class CompletedTrialBlockAjaxViewModel
    {
        public Word[] CorrectChoices { get; set; }
        public int Duration { get; set; }

        public int[] Latencies { get; set; }
        public string[] TranslationsClicked { get; set; }
        public Word[] WordsDisplayed { get; set; }
        public Word[][] OptionsDisplayed { get; set; }
    }
}