using alfariq.Models;
using alfariq.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class CompletedSessionViewModel
    {
        public string SessionName { get; set; }

        public string Name { get; set; }

        public string FeedbackMode { get; set; }

        public string ParticipantUsername { get; set; }

        public List<CompletedTrialBlockViewModel> TrialBlocks { get; set; }

        public CompletedSessionViewModel() { }

        public CompletedSessionViewModel(Session s, IQueryable<Profile> profileOptions)
        {
            TrialBlocks = new List<CompletedTrialBlockViewModel>();
            SessionName = s.Name;
            FeedbackMode = s.FeedbackCondition.Name;
            Name = s.Name;
            ParticipantUsername = s.Participant.Username;
            foreach (var block in s.TrialBlocks)
            {
                var blockVM = new CompletedTrialBlockViewModel(block, profileOptions);
                TrialBlocks.Add(blockVM);
            }
            TrialBlocks = TrialBlocks.OrderBy(x => x.IndexInSession).ToList();
        }
    }

    public class CompletedTrialBlockViewModel
    {
        public int Duration { get; set; }

        public int Id { get; set; }

        public int IndexInSession { get; set; }

        public List<DropdownOption> PassProfiles { get; set; }

        public List<DropdownOption> FailProfiles { get; set; }

        public List<CompletedTrialViewModel> Trials { get; set; }

        public CompletedTrialBlockViewModel(TrialBlock tb, IQueryable<Profile> profileOptions)
        {
            PassProfiles = new List<DropdownOption>();
            FailProfiles = new List<DropdownOption>();
            Trials = new List<CompletedTrialViewModel>();
            Duration = tb.Duration ?? 0;
            Id = tb.Id;
            IndexInSession = tb.IndexInSession;
            foreach (var p in tb.PassProfiles)
            {
                PassProfiles.Add(new DropdownOption(p));
            }
            foreach (var p in tb.FailProfiles)
            {
                FailProfiles.Add(new DropdownOption(p));
            }
            foreach (var t in tb.Trials)
            {
                Trials.Add(new CompletedTrialViewModel(t));
            }
            Trials = Trials.OrderBy(x => x.IndexInTrialBlock).ToList();
        }
    }

    public class CompletedTrialViewModel
    {
        public int IndexInTrialBlock { get; set; }

        public int Latency { get; set; }

        public string PromptedWord { get; set; }

        public string Option1 { get; set; }

        public string Option2 { get; set; }

        public string Option3 { get; set; }

        public string OptionPicked { get; set; }

        public bool Correct { get; set; }

        public CompletedTrialViewModel(Trial t)
        {
            OptionPicked = t.Answer.English;
            IndexInTrialBlock = t.IndexInTrialBlock ?? 0;
            Latency = t.Latency ?? 0;
            Option1 = t.Option1.English;
            Option2 = t.Option2.English;
            Option3 = t.Option3.English;
            PromptedWord = t.Word.Arabic;
            Correct = (t.Word.English == t.Answer.English);
        }
    }
}