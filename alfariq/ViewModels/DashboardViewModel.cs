using alfariq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class DashboardViewModel
    {
        public List<SessionPreviewModel> PendingSessions { get; set; }
        public List<SessionPreviewModel> CompletedSessions { get; set; }

        public DashboardViewModel(IQueryable<Session> sessionData)
        {
            PendingSessions = new List<SessionPreviewModel>();
            CompletedSessions = new List<SessionPreviewModel>();
            foreach (var s in sessionData)
            {
                if (s.Completed)
                {
                    CompletedSessions.Add(new SessionPreviewModel(s));
                }
                else
                {
                    PendingSessions.Add(new SessionPreviewModel(s));
                }
            }
        }
    }
}