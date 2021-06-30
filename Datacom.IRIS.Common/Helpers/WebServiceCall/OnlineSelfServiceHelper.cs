using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Datacom.IRIS.Common.Helpers
{
    public class SphereAuthenticationHelper
    {     
        #region GetIdentityToken

        public static string GetIdentityToken()
        {
            AcceptAllCertificatePolicy policy = new AcceptAllCertificatePolicy();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = policy.ValidateServerCertificate;

            // Username or cert binding
            var binding = new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential);

            // Username or cert factory
            var factory = new Microsoft.IdentityModel.Protocols.WSTrust.WSTrustChannelFactory(binding, ConfigurationManager.AppSettings[ConfigSettings.SphereIdentityProviderUrl]);

            factory.TrustVersion = TrustVersion.WSTrust13;
            var rst = new RequestSecurityToken
            {
                RequestType = WSTrust13Constants.RequestTypes.Issue,
                KeyType = WSTrust13Constants.KeyTypes.Bearer,
                AppliesTo = new EndpointAddress(ConfigurationManager.AppSettings[ConfigSettings.SphereIdentityProviderAppliesToUrl]) //ie. the URL that the requested security token applies to 
            };

            // set Username/password or cert 
            factory.Credentials.UserName.UserName = ConfigurationManager.AppSettings[ConfigSettings.SphereIdentityProviderUserName];
            factory.Credentials.UserName.Password = ConfigurationManager.AppSettings[ConfigSettings.SphereIdentityProviderPassword];
            
            factory.ConfigureChannelFactory();

            var token = factory.CreateChannel().Issue(rst) as GenericXmlSecurityToken;
            return token.TokenXml.OuterXml;
        }

        internal class AcceptAllCertificatePolicy : System.Net.ICertificatePolicy
        {

            public AcceptAllCertificatePolicy() { }

            public bool CheckValidationResult(System.Net.ServicePoint sp,
                                              System.Security.Cryptography.X509Certificates.X509Certificate cert,
                                              System.Net.WebRequest req,
                                              int certProb)
            {
                return true;
            }
            public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
               return true;
            }

        }
        #endregion
    }

}
