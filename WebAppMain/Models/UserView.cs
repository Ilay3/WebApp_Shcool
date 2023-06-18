using Microsoft.AspNetCore.Identity;
using System.Collections;
using System.Collections.Generic;
using WebAppMain.Enums;

namespace WebAppMain.Models
{
    public class UserView
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
        public IEnumerable<Class> Class { get; set; }
    }
}
