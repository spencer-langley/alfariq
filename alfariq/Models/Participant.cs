//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace alfariq.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Participant
    {
        public Participant()
        {
            this.Sessions = new HashSet<Session>();
        }
    
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Home { get; set; }
        public string ImagePath { get; set; }
    
        public virtual ICollection<Session> Sessions { get; set; }
    }
}