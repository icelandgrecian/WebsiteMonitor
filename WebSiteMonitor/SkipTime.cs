using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    public class SkipTime
    {
        public SkipTime(string details)
        {
            string[] timeDetails = details.Split('-');

            DayOfWeek dayOfWeek = DayOfWeek.Sunday;

            if (Enum.TryParse<DayOfWeek>(timeDetails[0], out dayOfWeek))
            {
                DayOfWeek = dayOfWeek;
            }

            DateTime startDateTime = DateTime.Parse(timeDetails[1]);
            DateTime endTDateTime = DateTime.Parse(timeDetails[2]);

            StartTime = new TimeSpan(startDateTime.Hour, startDateTime.Minute, 0);
            EndTime = new TimeSpan(endTDateTime.Hour, endTDateTime.Minute, 0);
        }   

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsInTime(DateTime dateTime)
        {
            bool result = false;

            if (dateTime.DayOfWeek == this.DayOfWeek)
            {
                result = CalculateDalUren(dateTime, StartTime, EndTime);
            }

            return result;
        }

        private bool CalculateDalUren(DateTime datum, TimeSpan? dalStart, TimeSpan? dalEnd)
        {
            bool isDal = false;
            DateTime StartDate = DateTime.Today;
            DateTime EndDate = DateTime.Today;

            //Check whether the dalEnd is lesser than dalStart
            if (dalStart >= dalEnd)
            {
                //Increase the date if dalEnd is timespan of the Nextday 
                EndDate = EndDate.AddDays(1);
            }

            //Assign the dalStart and dalEnd to the Dates
            StartDate = StartDate.Date + dalStart.Value;
            EndDate = EndDate.Date + dalEnd.Value;

            if ((datum >= StartDate) && (datum <= EndDate))
            {
                isDal = true;
            }
            return isDal;
        }
    }
}
