using alfariq.Models;
using alfariq.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class SessionViewModel
    {
        public string SessionID { get; set; }

        public string Name { get; set; }

        public int FeedbackModeID { get; set; }

        public int ParticipantID { get; set; }

        public List<DropdownOption> FeedbackOptions { get; set; }

        public List<DropdownOption> ParticipantOptions { get; set; }

        public List<TrialBlockViewModel> TrialBlocks { get; set; }

        public bool Completed { get; set; }

        public SessionViewModel() { }

        public SessionViewModel(IQueryable<FeedbackCondition> feedbackOptions, IQueryable<Profile> profileOptions, IQueryable<Participant> participantOptions, Session existingSession = null)
        {
            bool setValues = (existingSession != null);
            FeedbackOptions = new List<DropdownOption>();
            foreach (var opt in feedbackOptions)
            {
                var dropdownOption = new DropdownOption(opt);
                if (setValues && existingSession.FeedbackCondition.Id == opt.Id)
                {
                    dropdownOption.DefaultSelected = true;
                }
                FeedbackOptions.Add(dropdownOption);
            }
            ParticipantOptions = new List<DropdownOption>();
            foreach (var opt in participantOptions)
            {
                var dropdownOption = new DropdownOption(opt);
                if (setValues && existingSession.Participant.Id == opt.Id)
                {
                    dropdownOption.DefaultSelected = true;
                }
                ParticipantOptions.Add(dropdownOption);
            }
            TrialBlocks = new List<TrialBlockViewModel>();
            if (setValues)
            {
                SessionID = existingSession.Id.ToString();
                Name = existingSession.Name;
                foreach (var block in existingSession.TrialBlocks)
                {
                    var blockVM = new TrialBlockViewModel(block, profileOptions);
                    TrialBlocks.Add(blockVM);
                }
                Completed = existingSession.Completed;
            }
            else
            {
                Completed = false;
                SessionID = null;
                for (int x = 0; x < 8; x++)
                {
                    var newTB = new TrialBlockViewModel(profileOptions);
                    newTB.IndexInSession = x;
                    TrialBlocks.Add(newTB);
                }
            }
            TrialBlocks = TrialBlocks.OrderBy(x => x.IndexInSession).ToList();
        }
    }
}