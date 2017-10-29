using ImmunizationScheduleGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ImmunizationScheduleGenerator
{
    public class EventController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage  GetPatientCalendarFile(string name, string birthday)
        {
            var data = GenerateSchedule(name, birthday);
            var stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(stream.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = name+birthday + ".ics" // super simplify file name for demo purpose
                };

            return result;
        }

        // POST api/GenerateSchedule (for multiple patient in one call) 
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

        private string GenerateSchedule(string name, string birthday)
        {
            ImmunizationScheduleGenerator generator = new ImmunizationScheduleGenerator();
            DateTime dtBirthday = Convert.ToDateTime(birthday);
            string icsContent = generator.ICSFileGenerator(name, dtBirthday);
            return icsContent;
        }
    }
}
