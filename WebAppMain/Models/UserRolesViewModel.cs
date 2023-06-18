using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebAppMain.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<string> Class { get; set; }
    }
}
