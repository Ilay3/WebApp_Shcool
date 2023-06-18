using System.Collections.Generic;

namespace WebAppMain.Models
{
    public class JournalPredmet
    {
        public int Id { get; set; }
        public int JournalId { get; set; }
        public int PredmetId { get; set; }
        public Journal Journal { get; set; }
        public Predmet Predmet { get; set; }
        public ICollection<Estimation> Estimations { get; set; }
    }
}
