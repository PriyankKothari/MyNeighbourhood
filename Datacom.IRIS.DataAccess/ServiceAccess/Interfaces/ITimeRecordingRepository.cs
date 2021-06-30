using System;
using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ITimeRecordingRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        TimeRecord GetTimeRecordById(long id);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<TimeRecord> GetTimeRecordsGreaterThan(DateTime sinceDateTime, string accountName);
    }
}