namespace Fintech.AppCode.StaticModel
{
    public class SMSResponseTYPE
    {
        public static int SEND = 1;
        public static int UNSENT = 2;
        public static int DELIVERED = 3;
        public static int UNDELIVERED = 4;
        public static int FAILED = -1;
        public static int RESEND = 5;
    }
}
