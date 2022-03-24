using Fintech.AppCode.HelperClass;
using GoogleAuthenticatorService.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.HelperClass
{
    public class GoogleAuthenticatorManager
    {
        private const string Issuer = "RoundpayVoiceTech";
        private readonly string ProjectName;
        public GoogleAuthenticatorManager()
        {
            ProjectName = this.GetType().Assembly.FullName.Split(',')[0];
        }

        public SetupCode LoadSharedKeyAndQrCodeUrl(string userId, string AuthenticatorKey = "")
        {
            var guId = Guid.NewGuid();
            StringBuilder sb = new StringBuilder(guId.ToString());
            sb.Replace("-", "").Replace("{", "").Replace("}", "");
            AuthenticatorKey = String.IsNullOrEmpty(AuthenticatorKey) ? sb.ToString() : AuthenticatorKey;
            TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
            //var SetupResult = Authenticator.GenerateSetupCode(Issuer, ProjectName, AuthenticatorKey, 250, 250);
            var SetupResult = Authenticator.GenerateSetupCode(ProjectName, userId, AuthenticatorKey, 250, 250);
            return SetupResult;
        }
    }
}
