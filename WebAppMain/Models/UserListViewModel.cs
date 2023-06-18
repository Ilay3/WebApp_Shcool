using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppMain.Models
{
    public class UserListViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
        public IEnumerable<Class> Classes { get; set; }
        public UserManager<ApplicationUser> UserManager { get; set; }
        public RoleManager<IdentityRole> RoleManager { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string SearchString { get; set; }
        public string ClassName { get; set; }
    }
}
