using alfariq.Models;
using alfariq.ViewModels;
using alfariq.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace alfariq.Controllers
{
    public class HomeController : Controller
    {
        Dictionary<string, string> partPasswordsByUsername;

        public HomeController()
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            partPasswordsByUsername = new Dictionary<string, string>();
            foreach (var part in entities.Participants)
            {
                partPasswordsByUsername.Add(part.Username, part.Password);
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View("Index", new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginModel credentials)
        {
            if (credentials != null && partPasswordsByUsername.ContainsKey(credentials.Username) && partPasswordsByUsername[credentials.Username] == credentials.Password)
            {
                Session["User"] = credentials.Username;
                return Welcome();
            }
            credentials.Message = "Invalid login";
            return View(credentials);
        }

        public ActionResult Welcome()
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            if (sessionUser != null && partPasswordsByUsername.ContainsKey(sessionUser))
            {
                var participant = entities.Participants.Where(x => x.Username == sessionUser).SingleOrDefault();
                return View("Welcome", new ParticipantWelcomeViewModel(participant));
            }
            return Index();
        }

        public ActionResult RunSession(int sessionID)
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            if (sessionUser != null && partPasswordsByUsername.ContainsKey(sessionUser))
            {
                var sessionRecord = entities.Sessions.Where(x => x.Id == sessionID).SingleOrDefault();
                return View("RunSession", new ConductSessionViewModel(sessionRecord, entities.Words, entities.Profiles));
            }
            return Index();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SaveSession(CompletedSessionAjaxViewModel completedSession)
        {
            var reply = new AjaxResponse();
            reply.Success = false;
            reply.Message = "Nothing happened";

            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            if (sessionUser != null && partPasswordsByUsername.ContainsKey(sessionUser))
            {
                var sessionRecord = entities.Sessions.Where(x => x.Id == completedSession.SessionId).SingleOrDefault();

                if (sessionRecord == null)
                {
                    reply.Message = "Session ID not found";
                    return Json(reply);
                }
                if (sessionRecord.Participant.Username != sessionUser)
                {
                    reply.Message = "Requested session not affliated with user";
                    return Json(reply);
                }
                return Json(ProcessSessionRequest(completedSession));
            }
            return Json(reply);
        }

        private AjaxResponse ProcessSessionRequest(CompletedSessionAjaxViewModel completedSession)
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            var reply = new AjaxResponse();
            reply.Success = false;
            reply.Message = "Failed to save to database";

            Dictionary<string,Word> WordsByEnglish = new Dictionary<string,Word>();
            foreach(var w in entities.Words)
            {
                if (!WordsByEnglish.ContainsKey(w.English))
                {
                    WordsByEnglish.Add(w.English, w);
                }
            }

            var sessionToUpdate = entities.Sessions.Where(x => x.Id == completedSession.SessionId).SingleOrDefault();
            if(sessionToUpdate != null)
            {
                int tbi = 0;
                foreach(var tb in sessionToUpdate.TrialBlocks)
                {
                    int TrialCount = completedSession.TrialBlocks[tbi].Latencies.Length;
                    tb.Duration = completedSession.TrialBlocks[tbi].Duration;

                    for(int ti = 0; ti < TrialCount; ti++)
                    {
                        var newTrial = new Trial();
                        newTrial.Answer = WordsByEnglish[completedSession.TrialBlocks[tbi].TranslationsClicked[ti]];
                        newTrial.IndexInTrialBlock = ti;
                        newTrial.Latency = completedSession.TrialBlocks[tbi].Latencies[ti];
                        newTrial.Word = completedSession.TrialBlocks[tbi].WordsDisplayed[ti];

                        newTrial.Option1 = completedSession.TrialBlocks[tbi].OptionsDisplayed[ti][0];
                        newTrial.Option2 = completedSession.TrialBlocks[tbi].OptionsDisplayed[ti][1];
                        newTrial.Option3 = completedSession.TrialBlocks[tbi].OptionsDisplayed[ti][2];

                        tb.Trials.Add(newTrial);
                    }
                }
                sessionToUpdate.Completed = true;
                entities.SaveChanges();
                reply.Success = true;
                reply.Message = "Session saved";
            }
            return reply;
        }
    }
}
