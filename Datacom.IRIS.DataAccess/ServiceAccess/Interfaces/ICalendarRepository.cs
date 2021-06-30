using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ICalendarRepository : IRepositoryBase  
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Calendars, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]  
        List<Calendar> GetCalendarList(bool ShowInactive);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Calendars, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]  
        Calendar GetCalendarById(long id);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Calendars, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]  
        ObjectTypeCalendar GetObjectTypeCalendarById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Calendars, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]  
        List<ObjectTypeCalendar> GetObjectTypeCalendarList(bool ShowAll);

        [DoNotGenerateBusinessWrapperAttribute]
        List<NonWorkDay> GetWeekendsForCalendarId(long calendarId);  //only used by the business layer

        [DoNotGenerateBusinessWrapperAttribute]
        List<ObjectTypeCalendar> GetObjectTypeCalendarsByCalendarId(long calendarId);  //only used by the business layer

        [DoNotGenerateBusinessWrapperAttribute]
        void PopulateWeekends(long? calendarId, DateTime loopDate, DateTime endDate); //only used by the business layer
    }
}
