using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImmunizationScheduleGenerator.Models
{
    public class PatientModel
    {
        //public PatientModel ()
        //{
        //    this.NameAndBirthday = new Dictionary<string, string>();
        //}

        public Dictionary<string, string> NameAndBirthday { get; set; }
    }
}