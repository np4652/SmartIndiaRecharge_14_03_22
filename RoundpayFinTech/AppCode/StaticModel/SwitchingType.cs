namespace RoundpayFinTech.AppCode.StaticModel
{
    public static class SwitchingType
    {
        public const int UserwiseSwitching = 1;
        public const int RealAPIPerTransaction = 2;
        public const int CircleSwitching = 3;
        public const int DenominationSwitching = 4;
        public const int APISwitching = 5;
        public const int BackupAPISwitching = 6;
        public const int RealAPISwtiching = 7;
        public const int DenominationRangeSwitching = 8;
        public const int UserDenominationSwitching = 9;
        public const int UserDenominationRangeSwitching = 10;

        public static string GetSwitchType(int t) {
            switch (t) {
                case UserwiseSwitching:
                    return nameof(UserwiseSwitching);
                case RealAPIPerTransaction:
                    return nameof(RealAPIPerTransaction);
                case CircleSwitching:
                    return nameof(CircleSwitching);
                case DenominationSwitching:
                    return nameof(DenominationSwitching);
                case APISwitching:
                    return nameof(APISwitching);
                case BackupAPISwitching:
                    return nameof(BackupAPISwitching);
                case RealAPISwtiching:
                    return nameof(RealAPISwtiching);
                case DenominationRangeSwitching:
                    return nameof(DenominationRangeSwitching);
                case UserDenominationSwitching:
                    return nameof(UserDenominationSwitching);
                case UserDenominationRangeSwitching:
                    return nameof(UserDenominationRangeSwitching);
                default:
                    return string.Empty;

            }
        }
    }
}
