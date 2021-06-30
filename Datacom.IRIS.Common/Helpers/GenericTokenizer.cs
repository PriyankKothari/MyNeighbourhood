using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Datacom.IRIS.Common.Helpers
{
    public class GenericTokenizer 
    {

        protected Dictionary<string,string> TokensToReplace;

        public GenericTokenizer ()
        {
            TokensToReplace = new Dictionary<string,string>(); 
        }
        public GenericTokenizer (Dictionary<string,string> tokensToReplace)
        {
            TokensToReplace = tokensToReplace; 
        }

        /// <summary>
        /// Default TokenPattern is  {xxxx}
        /// </summary>
        /// <returns></returns>
        protected virtual Regex TokenPattern()
        {
            return new Regex(@"\{[^{}]+\}");
        }

        /// <summary>
        /// Return the token without the delimiter.  For example, if the matching token is {IRISObject.ID} we remove {} and return IRISObject.ID
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        protected virtual string RemoveTokenDelimiter(Match match)
        {
            return match.Value.Substring(1, match.Value.Length - 2);
        }

        protected virtual string EvalulateToken(string token)
        {
            string replacedValue;
            if (TokensToReplace.TryGetValue(token, out replacedValue))
                return replacedValue;
            else
                throw new ApplicationException("Token " + token + " cannot be evaluated.");               
        }

        
        public string ReplaceTokens(string text)
        {
            Regex tokenMatch = TokenPattern();
            return tokenMatch.Replace(text, new MatchEvaluator(this.ReplaceToken));
        }

        public string[] ExtractTokens(string text)
        {
            Regex tokenMatch = TokenPattern();
            var matches = tokenMatch.Matches(text);
            string[] tokens = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                tokens[i] = matches[i].Value;
            }
            return tokens;
        }

        private string ReplaceToken(Match match)
        {
            return EvalulateToken(RemoveTokenDelimiter(match));
        }



        
    }
}
