using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class JWTTokenModel
    {
        public object actor { get; set; }
        public object audiences { get; set; }
        public object claims { get; set; }
        public string encodedHeader { get; set; }
        public string encodedPayload { get; set; }
        public object header { get; set; }
        public string id { get; set; }
        public string issuer { get; set; }
        public string innerToken { get; set; }
        public string rawAuthenticationTag { get; set; }
        public string rawCiphertext { get; set; }
        public string rawData { get; set; }
        public string rawEncryptedKey { get; set; }
        public string rawInitializationVector { get; set; }
        public string rawHeader { get; set; }
        public string rawSignature { get; set; }
        public string securityKey { get; set; }
        public string signatureAlgorithm { get; set; }
        public string signingCredentials { get; set; }
        public string encryptingCredentials { get; set; }
        public string signingKey { get; set; }
        public string subject { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
    }
}
