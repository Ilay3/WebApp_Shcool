using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppMain.Models
{
    public class Predmet
    {
        [Key]
        public int Id_Predmet { get; set; }

        [Required]
        public string Title_Predmet { get; set; }
        public string Description_Predmet { get; set; }
        [Required]
        public string Study_period {get; set;}

        public virtual ICollection<JournalPredmet> JournalPredmets { get; set; }
    }

}
