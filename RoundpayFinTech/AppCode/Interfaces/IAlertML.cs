using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IAlertML
    {
        #region Bulk
        IResponseStatus SendSMS(int APIID, AlertReplacementModel model);
        IResponseStatus SendEmail(AlertReplacementModel model);
        Task BulkWebNotification(AlertReplacementModel param);
        IResponseStatus SendSocialAlert(AlertReplacementModel model, List<string> APIIDs);
        #endregion

        #region Registration
        IResponseStatus RegistrationSMS(AlertReplacementModel param);
        IResponseStatus RegistrationEmail(AlertReplacementModel param);
        #endregion

        #region FundTransfer
        IResponseStatus FundTransferSMS(AlertReplacementModel param);
        IResponseStatus FundTransferEmail(AlertReplacementModel param);
        IResponseStatus FundTransferNotification(AlertReplacementModel param);
        #endregion

        #region FundReceive
        IResponseStatus FundReceiveSMS(AlertReplacementModel param);
        IResponseStatus FundReceiveEmail(AlertReplacementModel param);
        IResponseStatus FundReceiveNotification(AlertReplacementModel param);
        #endregion

        #region FundCredit
        IResponseStatus FundCreditSMS(AlertReplacementModel param);
        IResponseStatus FundCreditEmail(AlertReplacementModel param);
        IResponseStatus FundCreditNotification(AlertReplacementModel param);
        #endregion

        #region FundDebit
        IResponseStatus FundDebitSMS(AlertReplacementModel param);
        IResponseStatus FundDebitEmail(AlertReplacementModel param);
        IResponseStatus FundDebitNotification(AlertReplacementModel param);
        #endregion

        #region OTP
        IResponseStatus OTPSMS(AlertReplacementModel param);
        IResponseStatus OTPEmail(AlertReplacementModel param);
        #endregion

        #region ForgetPassword
        IResponseStatus ForgetPasswordSMS(AlertReplacementModel param);
        bool ForgetPasswordEmail(AlertReplacementModel param);
        #endregion

        #region OperatorUpAndDown
        IResponseStatus OperatorUPSMS(AlertReplacementModel param, bool IsUp);
        IResponseStatus OperatorUpEmail(AlertReplacementModel param, bool IsUp);
        IResponseStatus OperatorUpNotification(AlertReplacementModel param, bool IsUp, bool IsBulk);
        #endregion

        #region FundOrder
        IResponseStatus FundOrderSMS(AlertReplacementModel param);
        IResponseStatus FundOrderEmail(AlertReplacementModel param);
        IResponseStatus FundOrderNotification(AlertReplacementModel param);
        #endregion

        #region KYCApprovedAndReject
        IResponseStatus KYCApprovalSMS(AlertReplacementModel param, bool IsApproved);
        IResponseStatus KYCApprovalEmail(AlertReplacementModel param, bool IsApproved);
        IResponseStatus KYCApprovalNotification(AlertReplacementModel param, bool IsApproved);
        #endregion

        #region RecharegeSuccessAndFailed
        IResponseStatus RecharegeSuccessSMS(AlertReplacementModel param, bool IsSuccess);
        IResponseStatus RecharegeSuccessEmail(AlertReplacementModel param, bool IsSuccess);
        IResponseStatus RecharegeSuccessNotification(AlertReplacementModel param, bool IsSuccess);
        #endregion

        #region RechargeRefund
        IResponseStatus RechargeRefundSMS(AlertReplacementModel param, bool IsRejected);
        IResponseStatus RechargeRefundEmail(AlertReplacementModel param, bool IsRejected);
        IResponseStatus RechargeRefundNotification(AlertReplacementModel param, bool IsRejected);
        #endregion

        #region LowBalanceAlert
        Task LowBalanceSMS(AlertReplacementModel param, SMSSetting smsSetting);
        Task LowBalanceEmail(AlertReplacementModel param, EmailSettingswithFormat mailSetting);
        Task LowBalanceNotification(AlertReplacementModel param);
        #endregion

        #region RecharegeAccept
        IResponseStatus RecharegeAcceptSMS(AlertReplacementModel param);
        IResponseStatus RecharegeAcceptEmail(AlertReplacementModel param);
        IResponseStatus RecharegeAcceptNotification(AlertReplacementModel param);
        #endregion

        #region CallNotPicked
        IResponseStatus CallNotPickedSMS(AlertReplacementModel param);
        IResponseStatus CallNotPickedEmail(AlertReplacementModel param);
        IResponseStatus CallNotPickedNotification(AlertReplacementModel param);
        #endregion

        #region UserPartialApproval
        Task UserPartialApprovalSMS(AlertReplacementModel param);
        Task UserPartialApprovalEmail(AlertReplacementModel param);
        #endregion
        IResponseStatus UserSubscription(AlertReplacementModel param);

        #region MarginRevised
        Task MarginRevisedSMS(AlertReplacementModel param);
        Task MarginRevisedEmail(AlertReplacementModel param);
        Task MarginRevisedNotification(AlertReplacementModel param, bool IsBulk);
        #endregion

        Task WebNotification(AlertReplacementModel param);
        Task SocialAlert(AlertReplacementModel param);
        #region BirthdayWishAlert
        IResponseStatus BirthdayWishSMS(AlertReplacementModel param, SMSSetting smsSetting);
        IResponseStatus BirthdayWishEmail(AlertReplacementModel param, EmailSettingswithFormat mailSetting);
        IResponseStatus BirthdayWishNotification(AlertReplacementModel param);
        #endregion

        IResponseStatus BBPSSuccessSMS(AlertReplacementModel param);
        IResponseStatus BBPSComplainRegistrationAlert(AlertReplacementModel param);

        IResponseStatus ResendSMS(string SendTo, string msg, AlertReplacementModel param = null);
    }
}
