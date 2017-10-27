using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ImmunizationScheduleGenerator
{
    public class ImmunizationScheduleGenerator
    {
        private const string vEventDateFormat = "yyyyMMdd";

        private string Name { get; set; }

        private DateTime Birthday { get; set; }

        private SortedList<int, string[]> VaccinationByMonthList;

        private List<string> NoVaccinationMonthList;
        

        public string ICSFileGenerator(string name, DateTime birthday)
        {
            Init(name, birthday);

            ReadSchedule();

            PopulateExceptionMonth();

            string content = GetICSContent();

            return content;
        }

        private void Init(string name, DateTime birthday)
        {
            this.VaccinationByMonthList = new SortedList<int, string[]>();
            this.NoVaccinationMonthList = new List<string>();
            this.Name = name;
            this.Birthday = birthday;
        }

        private string GetICSContent()
        {
            var vaccinationByMonthInstance = VaccinationByMonthList.ElementAt(0);
            var vEvents = GetVEvent();

            string content = string.Format(CalendarReminderTemplate.CalendarReminder,
                                        this.Name, //{0}
                                        string.Join(Environment.NewLine, vaccinationByMonthInstance.Value.Select(v => v.ToString())), //{1}
                                        this.Birthday.ToString(vEventDateFormat), //{2}
                                        this.Birthday.AddDays(1).ToString(vEventDateFormat), //{3}
                                        string.Join(",", this.NoVaccinationMonthList.Select(n => n.ToString())), //{4}
                                        this.Birthday.ToString("dd"),
                                        vEvents);

            return content;
        }

        private string GetVEvent()
        {
            StringBuilder sb = new StringBuilder();

            // skip the birth month
            for (int i = 1; i < this.VaccinationByMonthList.Count; i++)
            {
                var vaccinationByMonthInstance = VaccinationByMonthList.ElementAt(i);
                var vaccineStartDate = this.Birthday.AddMonths(vaccinationByMonthInstance.Key);

                string vEvent = string.Format(CalendarReminderTemplate.CalendarVEvent,
                                            this.Name, // person's name
                                            vaccinationByMonthInstance.Key, // vaccine month
                                            string.Join("\t", vaccinationByMonthInstance.Value.Select(v => v.ToString())), // list of vaccine needed
                                            vaccineStartDate.ToString(vEventDateFormat), //event occurence date
                                            vaccineStartDate.AddDays(1).ToString(vEventDateFormat), //event duration time
                                            vaccineStartDate.ToString("dd")); // event start date
                sb.AppendLine(vEvent);
            }

            return sb.ToString();
        }

        private void PopulateExceptionMonth()
        {
            int current = 0;
            foreach (var entry in this.VaccinationByMonthList)
            {
                int monthGap = entry.Key - current;
                if (monthGap > 1) // there are gaps in between months
                {
                    DateTime tmp = this.Birthday.AddMonths(entry.Key);
                    for (int i = 1; i < monthGap; i++)
                    {
                        this.NoVaccinationMonthList.Add(tmp.AddMonths(i * -1).ToString(vEventDateFormat));
                    }
                }
                current = entry.Key;
            }
        }

        private void ReadSchedule()
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/ImmunizationSchedule.txt");
            string[] lines = System.IO.File.ReadAllLines(path);

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    int commaSeparator = line.IndexOf(',');

                    int vaccineMonth = int.Parse(line.Substring(0, commaSeparator));

                    string[] vaccineTypes = line.Substring(commaSeparator + 1).Split('|');

                    this.VaccinationByMonthList.Add(vaccineMonth, vaccineTypes);
                }
            }
        }
    }
}