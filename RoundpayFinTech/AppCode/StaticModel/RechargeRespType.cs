namespace Fintech.AppCode.StaticModel
{
    public static class RechargeRespType
    {
        public const int PENDING = 1;
        public const int SUCCESS = 2;
        public const int FAILED = 3;
        public const int REFUND = 4;
        public const int REQUESTSENT = 5;

        public const int BALANCE = 6;
        public const int RESEND = 7;

        public const string _PENDING = "PENDING";
        public const string _SUCCESS = "SUCCESS";
        public const string _FAILED = "FAILED";
        public const string _FAILURE = "FAILURE";
        public const string _REFUND = "REFUND";
        public const string _REQUESTSENT = "REQUEST SENT";

        public const string _BALANCE = "BALANCE";
        public const string _RESEND = "RESEND";

        public static string GetRechargeStatusText(int Status) {
            if (PENDING == Status)
                return _PENDING;
            if (SUCCESS == Status)
                return _SUCCESS;
            if (FAILED == Status)
                return _FAILED;
            if (REFUND == Status)
                return _REFUND;
            if (REQUESTSENT == Status)
                return _REQUESTSENT;
            if (BALANCE == Status)
                return _BALANCE;
            if (RESEND == Status)
                return _RESEND;
            return "";
        }
    }

    public class StatusType
    {
        public const string SUCCESS = "SUCCESS";
        public const string FAIL = "FAIL";
    }
    public static class BBPSComplaintRespType
    {
        public const int PENDING = 1;
        public const int RESOLVED = 2;
        public const int FailedToResolved = 3;

        public const string _PENDING = "PENDING";
        public const string _RESOLVED = "RESOLVED";
        public const string _FailedToResolved = "Failed To Resolved";

        public static string GetComplaintStatusText(int Status)
        {
            if (PENDING == Status)
                return _PENDING;
            if (RESOLVED == Status)
                return _RESOLVED;
            if (FailedToResolved == Status)
                return _FailedToResolved;
            return "";
        }
    }

    public class BookingStatusType
    {
        public const int Requested = 1;
        public const int ForwardToEngineer = 2;
        public const int Installing = 3;
        public const int Completed = 4;
        public const int Rejected = 5;
    }
    public class RefundType
    {
        public static int DISPUTE = 1;
        public static int REQUESTED = 2;
        public static int REFUNDED = 3;
        public static int REJECTED = 4;
        public static string _DISPUTE = "DISPUTE";
        public static string _REQUESTED = "UNDER REVIEW";
        public static string _REFUNDED = "REFUNDED";
        public static string _REJECTED = "REJECTED";
        public static string GetRefundTypeText(int Status)
        {
            if (DISPUTE == Status)
                return _DISPUTE;
            if (REQUESTED == Status)
                return _REQUESTED;
            if (REFUNDED == Status)
                return _REFUNDED;
            if (REJECTED == Status)
                return _REJECTED;
            return "";
        }
    }
    public class OTPasswordStatus
    {
        public const string REQUIRED = "REQUIRED";
        public const string NOTREQUIRED = "NOT REQUIRED";
    }

    public class OTPasswordType
    {
        public const string SENDER = "sender";
        public const string BENEFICIARY = "beneficiary";
        public const string BENEFICIARY_REMOVE = "beneficiary_remove";
    }
    public class ReportType {
        public const int Recharge = 1;
        public const int DMR = 2;
        public const int AEPS = 22;
        public const int PaymentGateway = 31;
    }
}
