using alfariq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace alfariq.ViewModels
{
    public class SessionPreviewModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public bool Completed { get; set; }

        public SessionPreviewModel(Session data)
        {
            Name = data.Name;
            Id = data.Id;
            Completed = false;
        }
    }
}