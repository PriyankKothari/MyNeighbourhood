using System;
using System.Globalization;

namespace Datacom.IRIS.Common.Helpers
{
    public class DayLightSaving
    {
        private readonly TimeZone _zone;
        private readonly DaylightTime _dayLightTime;

        private DayLightSaving(int year)
        {
            _zone = TimeZone.CurrentTimeZone;
            _dayLightTime = _zone.GetDaylightChanges(year);
        }

        public static DayLightSaving New()
        {
            return new DayLightSaving(DateTime.Today.Year);
        }

        public static DayLightSaving New(int year)
        {
            return new DayLightSaving(year);
        }

        public DateTime StartDate{ get { return _dayLightTime.Start; } }
        public DateTime EndDate{ get { return _dayLightTime.End; } }
    }
}