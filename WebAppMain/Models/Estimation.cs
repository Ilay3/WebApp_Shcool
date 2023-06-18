using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppMain.Models
{
    public class Estimation
    {
        [Key]
        public int Id_Estimation { get; set; }
        public int Value { get; set; }

        public DateTime Date { get; set; }

        public int JournalPredmetId { get; set; }
        public virtual JournalPredmet JournalPredmet { get; set; }
    }
}
