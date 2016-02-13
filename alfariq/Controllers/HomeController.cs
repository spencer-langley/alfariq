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

        public HomeController()
        { }

        [HttpGet]
        public ActionResult Index()
        {
            return View("Index", new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginModel credentials)
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            if (credentials != null)
            {
                var dbUser = entities.Participants.Where(x => x.Username == credentials.Username && x.Password == credentials.Password).SingleOrDefault();
                if (dbUser != null)
                {
                    Session["User"] = credentials.Username;
                    return Welcome();
                }
            }
            credentials.Message = "Invalid login";
            return View(credentials);
        }

        public ActionResult Welcome()
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            var dbUser = entities.Participants.Where(x => x.Username == sessionUser).SingleOrDefault();
            if (dbUser != null)
            {
                return View("Welcome", new ParticipantWelcomeViewModel(dbUser));
            }
            return Index();
        }

        public ActionResult RunSession(int sessionID)
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            var dbUser = entities.Participants.Where(x => x.Username == sessionUser).SingleOrDefault();
            if (dbUser != null)
            {
                var sessionRecord = entities.Sessions.Where(x => x.Id == sessionID).SingleOrDefault();
                return View("RunSession", new ConductSessionViewModel(sessionRecord, entities.Words, entities.Profiles, dbUser.Username));
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
            var sessionUser = completedSession.Username;
            var dbUser = entities.Participants.Where(x => x.Username == sessionUser).SingleOrDefault();
            if (dbUser != null)
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
            reply.Message = "User \"" + sessionUser + "\"not found.";
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
