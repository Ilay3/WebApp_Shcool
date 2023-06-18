using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppMain.Models
{
    public class Class
    {
        [Key]
        public int Id_Class { get; set; }

        [Required(ErrorMessage = "Введите название класса")]
        public string Title_Class { get; set; }

        public ICollection<ApplicationUser> ApplicationUser { get; set; }
        public ICollection<Assignment> Assignment { get; set; }
    }
}
