namespace Fintech.AppCode.StaticModel
{
    public class LoginResponseCode
    {
        public const int FAILED = -1;
        public const int SUCCESS = 1;
        public const int OTP = 2;
        public const int Google2FAEnabled = 3;
    }
    public class OTPType {
        public const int LoginOTP = 1;
    }
}
