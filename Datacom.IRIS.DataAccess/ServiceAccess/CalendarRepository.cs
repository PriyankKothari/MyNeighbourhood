using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class CalendarRepository : RepositoryStore, ICalendarRepository
    {
        public List<Calendar> GetCalendarList(bool showInactive)
        {
            IQueryable<Calendar> queryable = showInactive ? Context.Calendar.Where(c => !c.IsDeleted) : Context.Calendar.Where(c => !c.IsDeleted && c.IsActive);
            return GetCalendarListFromQueryable(queryable);
        }

        public Calendar GetCalendarById(long id)
        {
            IQueryable<Calendar> queryable = Context.Calendar.Where(c => c.ID == id);
            return GetCalendarFromQueryable(queryable);
        }

        private List<Calendar> GetCalendarListFromQueryable(IQueryable<Calendar> queryable)
        {
            var dbquery = from calendar in queryable
                          select new
                          {
                              calendar,
                              nonWorkDays = from nonWorkDay in Context.NonWorkDay.Where(nw => !nw.IsDeleted && !nw.IsWeekEnd) select nonWorkDay,
                              objectTypeCalendar = from objectTypeCalendar in Context.ObjectTypeCalendar.Where(o => o.CalendarID == calendar.ID && !o.IsDeleted && (o.FromDate <= DateTime.Today.Date && (o.ToDate >= DateTime.Today.Date || o.ToDate == null))) select objectTypeCalendar
                          };
            return dbquery.AsEnumerable().Distinct().Select(c => c.calendar).ToList();
        }

        private Calendar GetCalendarFromQueryable(IQueryable<Calendar> queryable)
        {
            var dbquery = from calendar in queryable
                          select new
                          {
                              calendar,
                              nonWorkDays = from nonWorkDay in Context.NonWorkDay.Where(nw => !nw.IsDeleted && !nw.IsWeekEnd) select nonWorkDay,

                          };

            return dbquery.AsEnumerable().Select(c => c.calendar).FirstOrDefault().TrackAll();
        }

        public List<NonWorkDay> GetWeekendsForCalendarId(long calendarId)
        {
            return Context.NonWorkDay.Where(n => n.CalendarID == calendarId && !n.IsDeleted && n.IsWeekEnd).ToList();
        }

        public ObjectTypeCalendar GetObjectTypeCalendarById(long id)
        {
            return Context.ObjectTypeCalendar.Where(c => c.ID == id).Single().TrackAll();
        }

        public List<ObjectTypeCalendar> GetObjectTypeCalendarsByCalendarId(long calendarId)
        {
            return Context.ObjectTypeCalendar.Where(c => c.CalendarID == calendarId).ToList().TrackAll();
        }

        public List<ObjectTypeCalendar> GetObjectTypeCalendarList(bool showAll)
        {

            IQueryable<ObjectTypeCalendar> queryable = showAll ? Context.ObjectTypeCalendar.Where(c => !c.IsDeleted) : Context.ObjectTypeCalendar.Where(c => !c.IsDeleted && (c.ToDate >= DateTime.Today.Date || c.ToDate == null));

            var dbquery = from objectTypeCalendar in queryable
                          select new
                          {
                              objectTypeCalendar,
                              Calendar = objectTypeCalendar.Calendar, 
                              ReferenceDataValue = objectTypeCalendar.ObjectTypeREF,
                              ParentReferenceDataValue = objectTypeCalendar.ObjectTypeREF.ParentReferenceDataValue
                          };

            return dbquery.AsEnumerable().Select(c => c.objectTypeCalendar).ToList().TrackAll();
        }

        /// <summary>
        /// Generates weekend dates between given start and end dates.
        /// </summary>
        public void PopulateWeekends(long? calendarId, DateTime loopDate, DateTime endDate)
        {
            Context.PopulateWeekends(calendarId, loopDate, endDate, SecurityHelper.LoggedInUserAccountName);
        }
    }
}
