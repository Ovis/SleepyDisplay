using System;

namespace SleepyDisplay.Model
{
    public class DisplayTimeSetting
    {
        public DateTime ExecTime { get; set; }

        public PowerEnum Power { get; set; }

        public bool Enabled { get; set; }
    }
}
