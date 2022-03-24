namespace Fintech.AppCode.StaticModel
{
    public class SessionKeys
    {
        public const string LoginResponse = "LoginResponse";
        public const string AppSessionID = "App.SessionID";
        public const string LoginResponseEmp = "LoginResponseEmp";
        public const string AppSessionIDEmp = "App.SessionID.Emp";
        public const string ForgetOTP = "ForgetOTP";
        public const string CommonOTP = "CommonOTP";
        public const string RoleID = "Role";
        public const string OpAssigned = "OperationsAssigned";
        public const string WInfo = "WInfo";
        public const string Service = "SInfo";
        public const string EmailVerify = "EV";
        public const string RefID = "RefID";
        public const string AEPSSession = "AEPSSession";
        public const string NotificationID = "NotificationId";
        public const string RAILOTP = "RailOTP";
        public const string ECommUserDetail = "RPECommUserDetail";
        public const string ECommRecentView = "RPECommRecenView";
        public const string ECommLoginResponse = "ECommLoginResponse";
        public const string ECommAppSessionID = "App.ECommSessionID";
        public const string MobileOTP = "MobileOTP";
        public const string EmailOTP = "EmailOTP";
    }
    public class SessionType
    {
        public const int Single = 1;
        public const int Multiple = 2;
        public const int AllSessionSelf = 3;
        public const int AllSessionUser = 4;
    }
}
