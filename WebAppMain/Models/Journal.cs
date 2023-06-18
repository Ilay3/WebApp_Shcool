using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppMain.Models
{
    public class Journal
    {
        [Key]
        public int Id_Journal { get; set; }

        // Связи с другими таблицами
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<JournalPredmet> JournalPredmets { get; set; }
    }

}
