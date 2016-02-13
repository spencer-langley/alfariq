using alfariq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace alfariq.ViewModels
{
    public class ConductSessionViewModel
    {
        public int Id { get; set; }
        public string FeedbackCondition { get; set; }
        public string Username { get; set; }
        public List<ConductTrialBlockViewModel> TrialBlocks { get; set; }
        public string ProfilesJson { get; set; }
        public string WordsJson { get; set; }

        public ConductSessionViewModel(Session s, IEnumerable<Word> words, IEnumerable<Profile> profiles, string userName)
        {
            TrialBlocks = new List<ConductTrialBlockViewModel>();
            var AllProfiles = new List<ProfileViewModel>();
            Id = s.Id;
            Username = userName;
            FeedbackCondition = s.FeedbackCondition.Name;
            foreach (var block in s.TrialBlocks)
            {
                TrialBlocks.Add(new ConductTrialBlockViewModel(block));
            }
            foreach (var profile in profiles)
            {
                AllProfiles.Add(new ProfileViewModel(profile));
            }
            AllProfiles.Add(new ProfileViewModel(s.Participant));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ProfilesJson = serializer.Serialize((object)AllProfiles);
            WordsJson = serializer.Serialize((object)words);
        }
    }

    public class ConductTrialBlockViewModel
    {
        public int Id { get; set; }
        public int IndexInSession { get; set; }
        public List<ProfileViewModel> Fails { get; set; }
        public List<ProfileViewModel> Passes { get; set; }
        

        public ConductTrialBlockViewModel(TrialBlock tb)
        {
            Fails = new List<ProfileViewModel>();
            Passes = new List<ProfileViewModel>();

            foreach (var fail in tb.FailProfiles)
            {
                Fails.Add(new ProfileViewModel(fail));
            }
            foreach (var pass in tb.PassProfiles)
            {
                Passes.Add(new ProfileViewModel(pass));
            }
            Id = tb.Id;
            IndexInSession = tb.IndexInSession;
        }
    }

    public class ProfileViewModel
    {
        public int Age { get; set; }
        public string Home { get; set; }
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }

        public ProfileViewModel(Profile p)
        {
            Age = p.Age;
            Home = p.Home;
            Id = p.Id;
            ImagePath = p.ImagePath;
            Name = p.Name;
        }

        public ProfileViewModel(Participant p)
        {
            Age = p.Age;
            Home = p.Home;
            Id = -1;
            ImagePath = p.ImagePath;
            Name = p.Name;
        }
    }
}