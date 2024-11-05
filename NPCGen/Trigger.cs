using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcGen_Editor.Classes
{
    public class Trigger
    {
        public int Id;

        public int GmID;

        public string TriggerName;

        public byte AutoStart;

        public int WaitWhileStart;

        public int WaitWhileStop;

        public byte DontStartOnSchedule;

        public byte DontStopOnSchedule;

        public int StartYear;

        public int StartMonth;

        public int StartWeekDay;

        public int StartDay;

        public int StartHour;

        public int StartMinute;

        public int StopYear;

        public int StopMonth;

        public int StopWeekDay;

        public int StopDay;

        public int StopHour;

        public int StopMinute;

        public int Duration;
    }
}
