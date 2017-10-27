using ImmunizationScheduleGenerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace ImmunizationScheduleGenerator
{
    public class EventController : ApiController
    { 
        // POST api/GenerateSchedule
        public void GenerateSchedule([FromBody] PatientModel model)
        {
            ImmunizationScheduleGenerator generator = new ImmunizationScheduleGenerator();

            Dictionary<string, string> patientList = model.NameAndBirthday;

            foreach (var patient in patientList)
            {
                string name = patient.Key;
                DateTime birthday = Convert.ToDateTime(patient.Value);

                string icsContent = generator.ICSFileGenerator(name, birthday);

                string fileName = string.Format("{0}{1}.ics", name, birthday.ToString("yyyyMMdd"));
                using (FileStream fs = File.Create(HttpContext.Current.Server.MapPath("~/App_Data/" + fileName)))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(icsContent);
                    fs.Write(info, 0, info.Length);
                }
            }
        }
    }
}
