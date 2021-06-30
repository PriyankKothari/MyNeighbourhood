using System;
using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class TimeRecordingRepository : RepositoryStore, ITimeRecordingRepository {
        
        public TimeRecord GetTimeRecordById(long id)
        {
            return Context.TimeRecords
                   .Include(t => t.IRISObject.ObjectTypeREF)
                   .Include(t => t.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                   .Single(l => l.ID == id).TrackAll();            
        }

        /// <summary>
        ///    Return time records entered in the IRIS system greater than a given
        ///    date. This is primarily used by a web service that is exposed to financials
        ///    to retrieve recorded time info
        /// </summary>
        public List<TimeRecord> GetTimeRecordsGreaterThan(DateTime sinceDateTime, string accountName)
        {
            return Context.TimeRecords
                .Include(t => t.IRISObject)
                .Include(t => t.IRISObject.ObjectTypeREF)
                .Include(t => t.TimeCodeREF)
                .Where(t => t.Date >= sinceDateTime && (t.CreatedBy == accountName || String.IsNullOrEmpty(accountName)))
                .ToList();
        }
    }
}
