using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace WebAppMain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public byte[] ProfilePicture { get; set; }
        public DateTime Date_birth { get; set; }
        public int? Id_Class { get; set; }
        public Class Class { get; set; }
        public virtual Journal Journal { get; set; }

        public virtual ICollection<Note> Notes { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
