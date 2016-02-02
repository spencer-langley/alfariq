using alfariq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class ParticipantWelcomeViewModel
    {
        public ParticipantWelcomeViewModel(Participant p)
        {
            OpenSessions = new Dictionary<string, int>();
            this.Age = p.Age;
            this.Home = p.Home;
            this.Id = p.Id;
            this.ImagePath = p.ImagePath;
            this.Name = p.Name;
            this.Username = p.Username;
            foreach(var session in p.Sessions)
            {
                if (!session.Completed)
                {
                    OpenSessions.Add(session.Name, session.Id);
                }
            }
        }

        public int Age { get; set; }
        public string Home { get; set; }
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public Dictionary<string, int> OpenSessions { get; set; }

        public string Username { get; set; }
    }
}