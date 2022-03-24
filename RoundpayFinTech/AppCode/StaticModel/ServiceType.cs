namespace Fintech.AppCode.StaticModel
{
    public static class ServiceType
    {
        public const int Recharge = 1;
        public const int MoneyTransfer = 2;
        public const int Travel = 3;
        public const int FundTransfer = 4;
        public const int FundDeduction = 5;
        public const int AddWallet = 6;
        public const int Wallet = 7;
        public const int SMS = 8;
        public const int Rental = 9;
        public const int UserRegistration = 10;
        public const int BillPayment = 11;
        public const int AEPS = 17;
        public const int MiniBank = 34;
        public const int TMW = 18;
        public const int GenralInsurance = 19;
        public const int Commission = 20;
        public const int SubscriptionCharge = 21;
        public const int Shopping = 22;
        public const int PublicEService = 23;
        public const int Bidding = 24;
        public const int PSAService = 25;
        public const int Primary = 27;
        public const int Secondary = 28;
        public const int Tertiary = 29;
        public const int DTHSubscription = 30;
        public const int LeadService = 40;
        public const int CouponVouchers = 48;
        public const string RechargeReportServices = "1,11,19,22,23,25,48";
        public const string DMRReportServices = "2,32,38,39,42,43,46";
        public const string DayBookReportServices = "1,11,2";
        public const string AEPSResportService = "17,34";
        public const int ValueAddedService = 46;
        public const int AccountOpening = 47;
    }
    public static class ServiceCode
    {
        public const string Recharge = "RCH";
        public const string MoneyTransfer = "MT";
        public const string Travel = "TVL";
        public const string FundTransfer = "FTR";
        public const string FundDeduction = "FDT";
        public const string AddWallet = "AWT";
        public const string Wallet = "WT";
        public const string SMS = "SMS";
        public const string Rental = "REN";
        public const string UserRegistration = "REG";

        public const string AEPS = "AEP";
        public const string TMW = "TMW";
        public const string GenralInsurance = "GIS";
        public const string Commission = "COM";
        public const string SubscriptionCharge = "SCH";
        public const string Shopping = "SHP";
        public const string PublicEServices = "PES";
        public const string PSAService = "PSA";
        public const string BBPSService = "BPMT";
        public const string DTHSubscription = "DTHS";
        public const string MiniBank = "MNIBNK";
    }
    public static class PlanType
    {
        public const string CYRUS = "CYRUS";
        public const string VASTWEB = "VASTWEB";
        public const string MPLAN = "MPLAN";
        public const string Roundpay = "ROUNDPAY";
        public const string PLANAPI = "PLANAPI";
        public const string MYPLAN = "MYPLAN";
        public const string PLANSINFO = "PLANSINFO";
        public const string NoPLAN = "";
    }
    public static class DynamicWebsiteType {
        public const int Disabled = 1;
        public const int Corporate = 2;
        public const int Shopping = 3;
        public const int Travel = 4;
    }
    public static class LookupAPIType
    {
        public const string PLANAPI = "PLANAPI";
        public const string GoRecharge = "GORECH";
        public const string Roundpay = "ROUNDPAY";
        public const string APIBox = "APIBX";
        public const string MPLAN = "MPLAN";
        public const string MYPLAN = "MYPLAN";
        public const string VASTWEB = "VASTWEB";
        public const string INFOAPIHLR = "INFOAPIHLR";
        public const string AirtelPPHLR = "POSTAPHLR";
        public const string NoLOOKUP = "";
    }
    public static class RegistrationChargeType
    {
        public const int NOCharge = 1;
        public const int RegIdCount = 2;
        public const int RegCharge = 3;
    }
    public static class TargetType
    {
        public const int Servicewise = 1;
        public const int OpTypewise = 2;
        public const int Operatorwise = 3;
    }
    public static class FailoverApiOrder
    {
        public const int DefaultCount = 1;
        public const int FailoverCount = 2;
        public const int APIMargin = 3;
        
    }
}
