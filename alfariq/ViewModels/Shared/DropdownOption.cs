using alfariq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels.Shared
{
    public class DropdownOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool DefaultSelected { get; set; }

        public DropdownOption(FeedbackCondition condition)
        {
            Text = condition.Name;
            Value = condition.Id.ToString();
            DefaultSelected = false;
        }

        public DropdownOption(Profile p)
        {
            Text = p.Name;
            Value = p.Id.ToString();
            DefaultSelected = false;
        }

        public DropdownOption(Participant p)
        {
            Text = p.Name;
            Value = p.Id.ToString();
            DefaultSelected = false;
        }
    }
}