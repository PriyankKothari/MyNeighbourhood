using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common
{
    public struct WorkflowConstants
    {
        public const string AdHocTaskName = "_AdHocTask";

        public const string OfficerResponsible = "User:Officer Responsible";

        public const string UserPrefix = "User:";
        public const string GroupPrefix = "Group:";

        public const int RestartAtBeginningID = -1;
        public const string RestartAtBeginningText = "(Beginning)";

        public struct CreatedBy
        {
            public const string Workflow = "System";
            public const string Unknown = "Unknown";
        }

        public struct Outcomes
        {
            public const string Completed = "Completed";
        }
        /// <summary>
        /// Reference Dates are used to determine when a task is due.
        /// This structure contains the valid names of dates for this IRIS objects that have
        /// workflows associated with them.
        /// NOTE: These values must not contain whitespace as they are parsed as tokens from a string in the db.
        /// </summary>
        public struct ReferenceDates
        {
            public struct Common
            {
                public const string Created = "Created";
                public const string PreviousTaskCompletion = "PreviousTaskCompletion";
                public const string WorkflowStart = "WorkflowStart";
                public const string DueDate = "DueDate";

            }

            public struct Application
            {
                public const string LodgedDate = "LodgedDate";
                public const string NotificationDate = "NotificationDate";
                public const string SubmissionsCloseDate = "SubmissionsCloseDate";
                public const string HearingStartDate = "HearingStartDate";
                public const string PreApplicationDate = "PreApplicationDate";
            }

            public struct Authorisation
            {
                public const string IssuedDate = "IssuedDate";
                public const string GrantedDate = "GrantedDate";
            }

            public struct RegimeActivity
            {
                public const string TargetDateTo = "TargetDateTo";
            }

            public struct Request
            {
                public const string RequestDate = "RequestDate";
                public const string LGOIMADueDate = "LGOIMADueDate";
            }

            public struct EnforcementAction
            {
                public const string ServedDate = "ServedDate";
                public const string ReminderNoticeServedDate = "ReminderNoticeServedDate";
                public const string InformationSwornDueDate = "InformationSwornDueDate";
                public const string FinalDateForFilingChargingDocuments = "FinalDateForFilingChargingDocuments";
            }

            public struct SelectedLandUseSite
            {
                public const string TankPullDate = "TankPullDate";
            }

            public struct GeneralRegister
            {
                public const string RegisterDate = "RegisterDate";
            }

            public struct ManagementSite
            {
                public const string DataCreated = "DateCreated";
            }
        }

        public struct Tokens
        {
            public struct Application
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
            public struct Authorisation
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
            public struct RegimeActivity
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
            public struct Request
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
            public struct EnforcementAction
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
            public struct SelectedLandUseSite
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
            public struct ManagementSite
            {
                public const string OfficerResponsible = "OfficerResponsible";
            }
        }
    }
}
