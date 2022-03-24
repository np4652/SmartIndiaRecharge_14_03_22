using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcHotelBooking : IProcedure
    {
        private readonly IDAL _dal;
        public ProcHotelBooking(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new TekTvlError
            {
                ErrorCode = ErrorCodes.Minus1,
                ErrorMessage = ErrorCodes.TempError
            };
            var req = (HotelBook)obj;
                     SqlParameter[] param = {
                      new SqlParameter("@LT",req.LT),
                      new SqlParameter("@UserID",req.UserID),
                      new SqlParameter("@AccountNo",req.AccountNo),
                      new SqlParameter("@TokenID",req.TokenID),
                      new SqlParameter("@TraceID",(req.TraceID)),
                      new SqlParameter("@EndUserIP",req.EndUserIP),
                      new SqlParameter("@TotalAmount",req.TotalPrice),
                      new SqlParameter("@AvailabilityType",req.AvailabilityType),
                      new SqlParameter("@HotelCode",req.HotelCode),
                      new SqlParameter("@HotelName",req.HotelName),
                      new SqlParameter("@NoOfRooms",req.NoOfRooms),
                      new SqlParameter("@OPID",req.OPID),
                      new SqlParameter("@BookingId",req.BookingId),
                      new SqlParameter("@ConfirmationNo",req.ConfirmationNo),
                      new SqlParameter("@HotelBookingStatus",req.HotelBookingStatus),
                      new SqlParameter("@IsPriceChanged",req.IsPriceChanged),
                      new SqlParameter("@RequestModeID",req.RequestModeID),
                      new SqlParameter("@Childs",req.Childs),
                      new SqlParameter("@Adults",req.Adults),
                      new SqlParameter("@CheckInDate",req.CheckInDate),
                      new SqlParameter("@CheckOutDate",req.CheckOutDate),
                      new SqlParameter("@DestinationID",req.DestinationID),
                      new SqlParameter("@GuestDetails",req.DTGuestDetails)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    res.ErrorCode = 1;
                    res.ErrorMessage = "Message";
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                    // LoginTypeID = req.LoginTypeID,
                    // UserId = req.LoginTypeID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_HotelBooking";
    }

}
