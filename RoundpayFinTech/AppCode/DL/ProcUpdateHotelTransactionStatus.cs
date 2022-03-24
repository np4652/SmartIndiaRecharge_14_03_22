using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateHotelTransactionStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateHotelTransactionStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (HotelBook)obj;
            var res = new blockresponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@TID",req.TID),
                new SqlParameter("@HotelBookingStatus",req.HotelBookingStatus),
                new SqlParameter("@ConfirmationNo",req.ConfirmationNo),
                new SqlParameter("@BookingId",req.BookingId)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg= dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    res.IsError = true;
                    if (res.Statuscode == 1)
                    {
                        res.IsError = false;
                        res.Msg = "Success";
                        res.Msg=dt.Rows[0]["_TransactionID"] is DBNull ? "" : dt.Rows[0]["_TransactionID"].ToString();
                        res.Amount=dt.Rows[0]["_Amount"] is DBNull ? 0 :Convert.ToInt32(dt.Rows[0]["_Amount"]);
                        res.Msg=dt.Rows[0]["_HotelName"] is DBNull ? "" : dt.Rows[0]["_HotelName"].ToString();
                        res.NoOfRooms=dt.Rows[0]["_NoOfRooms"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_NoOfRooms"]);
                        res.NoOfChilds=dt.Rows[0]["_Childs"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Childs"]);
                        res.NoOfAdults=dt.Rows[0]["_Adults"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Adults"]);
                        res.Msg=dt.Rows[0]["_DestinationName"] is DBNull ? "" : dt.Rows[0]["_DestinationName"].ToString();
                        res.CheckInDate = Convert.ToDateTime("2021-11-08");
                        res.CheckOutDate = Convert.ToDateTime("2021-11-08");
                        res.BookingId = dt.Rows[0]["_BookingId"] is DBNull ? "" : dt.Rows[0]["_BookingId"].ToString();
                        res.ConfirmationNo = dt.Rows[0]["_ConfirmationNo"] is DBNull ? "" : dt.Rows[0]["_ConfirmationNo"].ToString();
                        res.TrnsactionID = dt.Rows[0]["_TransactionID"] is DBNull ? "" : dt.Rows[0]["_TransactionID"].ToString();
                        res.TrnsactionID = dt.Rows[0]["_TransactionID"] is DBNull ? "" : dt.Rows[0]["_TransactionID"].ToString();
                        res.TranDate = Convert.ToDateTime("2021-11-08");
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        // public string GetName() => "proc_SMSAPI_CU";
        public string GetName() => "proc_UpdateHotelTransactionStatus";
    }
}
