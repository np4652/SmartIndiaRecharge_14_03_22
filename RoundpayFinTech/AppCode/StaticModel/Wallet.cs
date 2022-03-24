namespace RoundpayFinTech.AppCode.StaticModel
{
    public static class Wallet
    {
        public const int Prepaid = 1;
        public const int Utility = 2;
        public const int Bank = 3;
        public const int Card = 4;
        public const int RegID = 5;
        public const int Package = 6;

        public const string _Prepaid = "Prepaid";
        public const string _Utility = "Utility";
        public const string _Bank = "MiniBank";
        public const string _Card = "PrepaidCard";
        public const string _RegID = "RegistrationID";
        public const string _Package = "Package";
        public const string _OSBalance = "OutStanding";

        public static string GetWalletType(int T) {
            if (T == Prepaid)
                return _Prepaid;
            if (T == Utility)
                return _Utility;
            if (T == Bank)
                return _Bank;
            if (T == Card)
                return _Card;
            if (T == RegID)
                return _RegID;
            if (T == Package)
                return _Package;
            return "";
        }

        public const string _PrepaidCapping = "Capping";
        public const string _UtilityCapping = "UCapping";
        public const string _BankCapping = "MBCapping";
        public const string _CardCapping = "CCapping";
        public const string _RegIDCapping = "IDCapping";
        public const string _PackageCapping = "PckgCapping";

        public const int MoveToPrepaidWallet = 1;
        public const int MoveToUtilityWallet = 2;
        public const int MoveToBank = 3;
    }
}
