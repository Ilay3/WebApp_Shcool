using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppMain.Models
{
    //Модель вывода журнала пользователя
    public class JournalViewModel
    {
        public ApplicationUser User { get; set; }
        public Journal Journal { get; set; }
        public Dictionary<string, double> AverageGradePerSubject { get; set; }

    }

    //Модель добавления оценки
    public class AddEstimationViewModel
    {
        public int JournalPredmetId { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }

    //модель добавления предмета
    public class AddPredmetViewModel
    {
        public string UserId { get; set; }
        public List<Predmet> Predmets { get; set; }
        public int PredmetId { get; set; }
    }

    //модель вывола списка оценок
    public class EstimationsViewModel
    {
        // Журнал, для которого отображаются оценки
        public Journal Journal { get; set; }
        public int JournalPredmetId { get; set; }


        // Предмет, для которого отображаются оценки
        public JournalPredmet JournalPredmet { get; set; }

        // Список оценок
        public List<Estimation> Estimations { get; set; }

        // Выбранный квартал
        public int SelectedQuarter { get; set; }
    }





}


