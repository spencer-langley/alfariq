using alfariq.Models;
using alfariq.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class TrialBlockViewModel
    {
        public List<DropdownOption> ProfileOptions { get; set; }

        public List<int> ProfilesIDsToPass { get; set; }

        public List<int> ProfilesIDsToFail { get; set; }

        public int IndexInSession { get; set; }

        public int? ID { get; set; }

        public TrialBlockViewModel()
        {
            ProfileOptions = new List<DropdownOption>();
            ProfilesIDsToFail = new List<int>();
            ProfilesIDsToPass = new List<int>();
        }

        public TrialBlockViewModel(IQueryable<Profile> profileOptions)
        {
            ProfilesIDsToFail = new List<int>();
            ProfilesIDsToPass = new List<int>();
            ProfileOptions = new List<DropdownOption>();
            foreach (var opt in profileOptions)
            {
                ProfileOptions.Add(new DropdownOption(opt));
            }
            ID = null;
        }

        public TrialBlockViewModel(TrialBlock coreModel, IQueryable<Profile> profileOptions)
        {
            ProfilesIDsToFail = new List<int>();
            ProfilesIDsToPass = new List<int>();
            ProfileOptions = new List<DropdownOption>();
            foreach (var opt in profileOptions)
            {
                ProfileOptions.Add(new DropdownOption(opt));
            }
            
            IndexInSession = coreModel.IndexInSession;
            foreach (var profile in coreModel.FailProfiles)
            {
                ProfilesIDsToFail.Add(profile.Id);
            }
            foreach (var profile in coreModel.PassProfiles)
            {
                ProfilesIDsToPass.Add(profile.Id);
            }
            ID = coreModel.Id;
        }
    }
}