using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppMain.Models
{
    public class EditUserViewModel
    {

        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> ClassList { get; set; }
        public int ClassId { get; set; }
    }
}
