using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datacom.IRIS.Common.Helpers;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.Helpers
{
    public class IRISObjectTokenizer : GenericTokenizer
    {
        private IRISObject IRISObject { get; set; }
        private Dictionary<string, string> AdditionalTokensToReplace { get; set; }

        public IRISObjectTokenizer (IRISObject irisObject)
        {
            IRISObject = irisObject;
            AdditionalTokensToReplace = new Dictionary<string, string>();
        }

        public IRISObjectTokenizer (IRISObject irisObject, Dictionary<string,string> additionalTokensToReplace)
        {
            IRISObject = irisObject;
            AdditionalTokensToReplace = additionalTokensToReplace;
        }

        protected override string EvalulateToken(string token)
        {
            if (token == "IRISRecordHyperlink")
                return GetIRISRouterUrl();
            else if (AdditionalTokensToReplace.ContainsKey(token))
                return AdditionalTokensToReplace[token];
            else
                return IRISObject.EvaluateToken(token);
        }

        private string GetIRISRouterUrl()
        {
            var appSettings = RepositoryMap.CommonRepository.GetAllAppSettings();

            return appSettings.First(x => x.Key == "IRISWebSiteURL").Value +
                   appSettings.First(x => x.Key == "IRISObjectRouterURLQueryString").Value +
                   IRISObject.ID;
        }

    }
}
