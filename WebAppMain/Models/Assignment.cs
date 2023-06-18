using System.ComponentModel.DataAnnotations;
using System;

namespace WebAppMain.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название задания")]
        public string Title { get; set; }
        public string Description { get; set; }
        public byte[] File { get; set; }
        public DateTime Date { get; set; }
        public bool IsPublic { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; }
    }

}
