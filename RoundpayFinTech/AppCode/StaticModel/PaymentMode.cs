

namespace Fintech.AppCode.StaticModel
{
    public static class PaymentMode_
    {
        public const int Neft = 1;
        public const int RTGS = 1;
        public const int IMPS = 1;
        public const int ThirdPartyTransfer = 2; //3;
        public const int CashDeposit = 3;//7;
        public const int CDM = 9;

        public const int GCC = 4;//8;
        public const int Cheque = 5;//9;
        public const int ScanAndPay = 6;//10;
        public const int UPI = 7;//11;
        public const int Exchange = 8;//7;
        public const int WalletTransfer = 9;//7;

        public const int PAYTM_Neft = 1;
        public const int PAYTM_RTGS = 2;
        public const int PAYTM_IMPS = 3;

        public const int Payout_Neft = 1;
        public const int Payout_RTGS = 2;
        public const int Payout_IMPS = 3;

    }

    public static class PaymentGatewayTranMode
    {
        public const string NetBanking = "NBNK";
        public const string DebitCard = "DCR";
        public const string CreditCard = "CCRD";
        public const string UPI = "UPI";
        public const string UPIAdmoney = "37UPI";
        public const string UPIICI = "UPIICI";
        public const string PPIWALLET = "PWLT";
    }

    public static class FlatTypeMaster {
        public const int FlatToAll = 1;
        public const int ByAdminOnly = 2;
        public const int DisableFlat = 3;
        public const int FlatToAllByDifference = 4;
    }
}
