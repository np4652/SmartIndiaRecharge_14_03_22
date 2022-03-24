using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class CertificateModel: CompanyProfileDetail
    {
        public string joiningDate { get; set; }
        public string OutletName { get; set; }
        public int OutletID { get; set; }
        public int WID { get; set; }
        
    }

    public class IrctcCertificateModel:CertificateModel
    {
        public string CompanyName { get; set; }
        public string IrctcID { get; set; }
        public string ExpDate { get; set; }
        public string URl { get; set; }
        public string BgImage { get; set; }
        public string LogoImage { get; set; }
        public string SignImage { get; set; }
       public int IrctcStatus { get; set; }

    }
}
