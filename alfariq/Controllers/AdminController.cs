﻿using alfariq.Models;
using alfariq.ViewModels;
using alfariq.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace alfariq.Controllers
{
    public class AdminController : Controller
    {
        //Models.db38bab79d27554b96b50aa57c010cd149Entities1 entities;
        Dictionary<string, string> adminPasswordsByUsername;

        public AdminController()
        {
            var entities = new Models.db38bab79d27554b96b50aa57c010cd149Entities3();
            adminPasswordsByUsername = new Dictionary<string, string>();
            foreach (var admin in entities.Admins)
            {
                adminPasswordsByUsername.Add(admin.Username, admin.Password);
            }
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model)
        {
            if (adminPasswordsByUsername.ContainsKey(model.Username) && adminPasswordsByUsername[model.Username] == model.Password)
            {
                Session["User"] = model.Username;
                return Dashboard();
            }

            model.Message = "Invalid login";
            return View(model);
        }

        public ActionResult Dashboard()
        {
            var entities = new db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            if (sessionUser != null && adminPasswordsByUsername.ContainsKey(sessionUser))
            {
                var viewModel = new DashboardViewModel(entities.Sessions);
                return View("Dashboard", viewModel);
            }
            return Index();
        }

        public ActionResult EditSession(int? sessionID)
        {
            var entities = new db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            if (sessionUser != null && adminPasswordsByUsername.ContainsKey(sessionUser))
            {
                SessionViewModel viewModel;
                if (sessionID == null)
                {
                    viewModel = new SessionViewModel(entities.FeedbackConditions, entities.Profiles, entities.Participants);

                }
                else
                {
                    var existingSession = entities.Sessions.Where(x => x.Id == sessionID).FirstOrDefault();
                    viewModel = new SessionViewModel(entities.FeedbackConditions, entities.Profiles, entities.Participants, existingSession);
                }
                return View(viewModel);
                
            }
            return Index();
        }

        public ActionResult ViewSessionData(int? sessionID)
        {
            var entities = new db38bab79d27554b96b50aa57c010cd149Entities3();
            var sessionUser = (string)Session["User"];
            if (sessionUser != null && adminPasswordsByUsername.ContainsKey(sessionUser) && sessionID != null)
            {
                var existingSession = entities.Sessions.Where(x => x.Id == sessionID).FirstOrDefault();
                var viewModel = new CompletedSessionViewModel(existingSession, entities.Profiles);
                return View(viewModel);

            }
            return Index();
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public ActionResult ImportProfiles(ProfileImportRequest request)
        //{
        //    var entities = new db38bab79d27554b96b50aa57c010cd149Entities3();

        //    string[] stringSeparators = new string[] { ", " };

        //    foreach (var w in request.profiles)
        //    {
        //        var newProfile = new Profile();

        //        var nameParts = w.name.Split(stringSeparators, StringSplitOptions.None);

        //        var ageParts = nameParts[1].Split(" ".ToCharArray());
        //        var home = nameParts[2].Replace("lives in ", string.Empty);

        //        newProfile.Name = nameParts[0];
        //        newProfile.Age = Int32.Parse(ageParts[0]);
        //        newProfile.Home = home;

        //        newProfile.ImagePath = "x";

        //        entities.Profiles.Add(newProfile);
        //        entities.SaveChanges();

        //        newProfile.ImagePath = "/Images/Profiles/" + newProfile.Id + ".jpg";
        //        entities.SaveChanges();
        //    }

        //    AjaxResponse response = new AjaxResponse();
        //    response.Success = true;
        //    response.Message = "It worked.";

        //    return Json(response);
        //}

        //public class ProfileImportRequest
        //{
        //    public List<ProfileRequest> profiles { get; set; }
        //}

        //public class ProfileRequest
        //{
        //    public string name { get; set; }
        //    public string image { get; set; }
        //}

        public ActionResult UpdateSession(SessionViewModel form)
        {
            AjaxResponse response = new AjaxResponse();
            response.Success = false;
            response.Message = "Nothing happened.";

            var entities = new db38bab79d27554b96b50aa57c010cd149Entities3();

            Session newSession;
            if (form.SessionID == null || form.SessionID == string.Empty)
            {
                newSession = new Session();
                entities.Sessions.Add(newSession);
            }
            else
            {
                int parsedID = Int32.Parse(form.SessionID);
                newSession = entities.Sessions.Where(x => x.Id == parsedID).FirstOrDefault();
            }

            newSession.Name = form.Name;

            var part = entities.Participants.Where(x => x.Id == form.ParticipantID).FirstOrDefault();
            var feedback = entities.FeedbackConditions.Where(x => x.Id == form.FeedbackModeID).FirstOrDefault();

            newSession.FeedbackCondition = feedback;
            newSession.Participant = part;
            entities.SaveChanges();

            foreach(var block in form.TrialBlocks)
            {
                TrialBlock newTB;
                
                if (block.ID == null)
                {
                    newTB = new TrialBlock();
                }
                else
                {
                    newTB = entities.TrialBlocks.Where(x => x.Id == block.ID).FirstOrDefault();
                    newTB.FailProfiles.Clear();
                    newTB.PassProfiles.Clear();
                }

                newTB.IndexInSession = block.IndexInSession;

                foreach (var id in block.ProfilesIDsToFail)
                {
                    var profile = entities.Profiles.Where(x => x.Id == id).FirstOrDefault();
                    newTB.FailProfiles.Add(profile);
                }

                foreach (var id in block.ProfilesIDsToPass)
                {
                    var profile = entities.Profiles.Where(x => x.Id == id).FirstOrDefault();
                    newTB.PassProfiles.Add(profile);
                }
                if (block.ID == null)
                {
                    newSession.TrialBlocks.Add(newTB);
                }
            }
            entities.SaveChanges();

            response.Success = true;
            response.Message = "Changes saved to database";

            return Json(response);
        }
    }
}