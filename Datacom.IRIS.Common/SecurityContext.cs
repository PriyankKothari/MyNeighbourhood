using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Datacom.IRIS.Common
{
    /// <summary>
    ///    A security context object is returned by every domain object that is an IIRISObject
    ///    to define to the IRIS backend the parameters that need to be used for running
    ///    security checks. The SecurityContext will typicall have a copy of IRISObject data.
    /// 
    ///    This object allows a domain object to override its default security context and use
    ///    return another security context were needed. Perfect scenario when a domain object
    ///    such as an activity does not have permissions of its own.
    /// </summary>
    [Serializable]
    public class SecurityContext
    {
        private bool _matchSubClass2ID = true;

        [DataMember]
        public long IRISObjectID { get; set; }

        [DataMember]
        public string ObjectTypeCode { get; set; }

        [DataMember]
        public long ObjectTypeID { get; set; }

        [DataMember]
        public long? SubClass1ID { get; set; }

        [DataMember]
        public long? SubClass2ID { get; set; }

        /// <summary>
        ///    By default the Authorisation Manager which consumes the SecurityContext object will always
        ///    match down to the SubClass2 level. There are some security contexts that do not want to be
        ///    checked down to that level, and essentially skip having their SubClass2ID checked
        /// 
        ///    Example: On the Create Authorisation Group form, the authorisation type dropdown needs to be
        ///    filtered according to a user's permissions. A user that has access to 
        ///    "Authorisation->Building Consent->Alter" should be able to see 'Building Consent' in the
        ///    dropdown.
        /// </summary>
        [DataMember]
        public bool MatchSubClass2ID
        {
            get { return _matchSubClass2ID; }
            set { _matchSubClass2ID = value; }
        }

        /// <summary>
        ///  The IRIS object ID this Security Context is meant for.
        ///  In most cases, IRISObjectID would be equal to ForIRISObjectID unless the object that is
        ///  inheriting security context from its parent.
        /// </summary>
        [DataMember]
        public long ForIRISObjectID { get; set; }

    }
}
