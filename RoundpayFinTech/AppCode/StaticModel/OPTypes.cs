using System.Collections.Generic;

namespace Fintech.AppCode.StaticModel
{
    public static class SPKeys
    {
        public const string AepsCashWithdrawal = "2201";
        public const string AepsCashDeposit = "2202";
        public const string Aadharpay = "2203";
        public const string AepsMiniStatement = "2204";
        public const string MATMCashWithdrawal = "4401";
        public const string MATMCashDeposit = "4402";
        public const string CashAtPOS = "1301";
        public const string CreditCardsStandard = "1302";
        public const string DebitCards = "1303";
        public const string MposEMI = "1304";
        public const string MposUPI = "1305";
        public const string MposWallet = "1306";
        public const string CreditCardPremium = "1307";
        public const string CreditCardInternational = "1308";
        public const string DebitCardRupay = "1309";
        public const string UpiVerification = "5901";
        public const string AccountVerification = "5902";
        public const string PANVerification = "5903";
        public const string HLRVerification = "5905";
        public const string GSTVerification = "5906";
        public const string AadhaarVerification = "5907";

        public const string IPGeoLocationInfo = "8001";

        //FOr PAYOUT
        public const string NEFT = "NEFT";
        public const string IMPS = "IMPS";
        public const string RTGS = "RTGS";
        public const string UPI = "UPI";
    }
    public static class OPTypes
    {
        public const int Prepaid = 1;
        public const int Postpaid = 2;
        public const int DTH = 3;
        public const int Landline = 4;
        public const int Electricity = 5;
        public const int Gas = 6;
        public const int DomesticHotel = 7;
        public const int InternationalHotel = 8;
        public const int DomesticFlight = 9;
        public const int InternationalFlight = 10;
        public const int Bus = 11;
        public const int Connection = 12;
        public const int MPOS = 13;
        public const int MiniATM = 44;
        public const int DMR = 14;
        public const int DMRCharge = 15;
        public const int Broadband = 16;
        public const int Water = 17;
        public const int Train = 18;
        public const int Shopping = 19;
        public const int PublicEServices = 20;
        public const int Bidding = 21;
        public const int AEPS = 22;
        public const int PSA = 24;
        public const int BikeInsurance = 28;
        public const int FourWheelerInsurance = 29;
        public const int Primary = 30;
        public const int Secondary = 31;
        public const int FRCPrepaid = 32;
        public const int FRCDTH = 33;
        public const int DTHSR = 34;
        public const int HDBOX = 35;
        public const int SDBOX = 36;
        public const int AddMoney = 37;
        public const int UPI = 50;
        public const int IndoNepalDMT = 51;
        public const int Hospital = 52;
        public const int PayoutDMT = 53;
        public const int HealthInsurance = 54;
        public const int LoanServiceLead = 55;
        public const int CreditCardLead = 56;
        public const int InsuranceLead = 57;
        public const int Verification = 59;
        public const int UPIPayment = 62;
        public static List<int> AllowToWhitelabel = new List<int>{
            Prepaid,Postpaid,DTH
        };
        public const int DTHConnection = 65;
        public const int DTHComplain = 66;
        public const int BankAccountOpening = 74;
        public const int CouponVoucher = 75;
        public const int Coin = 76;

        public const int RechargePlan = 67;
        public const int DTHPlan = 68;
        public const int ROffer = 77;
        public const int DTHCustInfo = 78;
        public const int DTHHeavyRefresh = 79;
        public const int Hotel = 41;
        public const int RealTimeBank = 42;

    }
    public static class CommStttingType
    {
        public const int Traditional = 1;
        public const int Rangewise = 2;
    }
    public class Deletable
    {
        public const int TblBank = 1;
    }
    public static class PGAgentType{
        public const int AgentOnly = 1;
        public const int CustomerOnly = 2;
        public const int Both = 3;
    }
    public class CIRCLE_VALIDATION
    {
        public const int NO = 0;
        public const int TABLE = 1;
        public const int API = 2;
    }
    public class ErrType
    {
        public const int Validation = 1;
        public const int SenderRegistration = 2;
        public const int BeneficiaryRegistration = 3;
        public const int Transaction = 4;
        public const int StatusCheck = 5;
        public const int GenericError = 6;
        public const int BillFetch = 7;
    }
    public class LogType
    {
        public const string ROFR = "ROFR";
        public const string BillFetch = "BillFetch";
        public const string APIURL = "APIURL";
        public const string HLR = "HLR";
    }
}
