using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class Package_Cl
    {
        public int ServiceID { get; set; }
        public int ParentID { get; set; }
        public bool IsServiceActive { get; set; }
        public bool IsShowMore { get; set; }
        public string Upline { get; set; }
        public string UplineMobile { get; set; }
        public string CCContact { get; set; }
        public string  Name { get; set; }
        public string  Service { get; set; }
        public string  SCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDisplayService { get; set; }
        public bool IsAdditionalServiceType { get; set; }
        public string ServiceTypeID { get; set; }
    }
    public class SellerDashboard
    {
        public List<Package_Cl> PackageCl { get; set;}
        public UserBalnace CurrentBal { get; set; }
        public bool IsMLM { get; set; }
    }
    public class UserBalnace
    {
        public bool IsShowLBA { get; set; }
        public decimal Balance {get; set;}
        public decimal BCapping { get; set; }
        public bool IsBalance { get; set; }
        public bool IsBalanceFund { get; set; }
        public decimal UBalance {get; set;}
        public decimal UCapping { get; set; }
        public bool IsUBalance { get; set; }
        public bool IsUBalanceFund { get; set; }
        public decimal BBalance {get; set;}
        public decimal BBCapping { get; set; }
        public bool IsBBalance { get; set; }
        public bool IsBBalanceFund { get; set; }
        public decimal CBalance {get; set;}
        public decimal CCapping { get; set; }
        public bool IsCBalance { get; set; }
        public bool IsCBalanceFund { get; set; }
        public decimal IDBalnace {get; set;}
        public decimal OSBalance { get; set;}
        public decimal IDCapping { get; set; }
        public bool IsIDBalance { get; set; }
        public bool IsIDBalanceFund { get; set; }
        public decimal PacakgeBalance { get; set; }
        public decimal PackageCapping { get; set; }
        public bool IsPacakgeBalance { get; set; }
        public bool IsPacakgeBalanceFund { get; set; }
        public bool IsP { get; set; }//IsPasswordExpiredOrNot
        public bool IsPN { get; set; }//_IsPINNotSet
        public bool IsLowBalance { get; set; }
        public bool IsFlatCommissionU { get; set; }
        public bool IsPackageDeducionForRetailor { get; set; }
        public bool IsAdminDefined { get; set; }//Channel or Level type user
        public decimal CommRate { get; set; }
        public string VIAN { get; set; }
        public bool InvoiceByAdmin { get; set; }
        public bool IsMarkedGreen { get; set; }
        public bool IsQRMappedToUser { get; set; }
        public bool IsCandebit { get; set; }
    }
    //public class Package_Master
    //{
    //    public List<Package_Cl> Package { get; set; }
    //}
}
