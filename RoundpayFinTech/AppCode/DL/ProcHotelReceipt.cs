using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
namespace RoundpayFinTech.AppCode.DL
{
    public class ProcHotelReceipt : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcHotelReceipt(IDAL dal) => _dal = dal;
        public string GetName() => "proc_HotelReceipt";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@BookingId", _req.CommonInt),
                new SqlParameter("@TID", _req.CommonInt2),
                new SqlParameter("@LoginID", _req.CommonInt3)
            };
            List<HotelReceiptGuest> HotelReceiptGuest = new List<HotelReceiptGuest>();
            var hotelreceipt = new HotelReceipt
            {
                IsError = true,
                Status = ErrorCodes.TempError
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                DataTable dt = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
                DataTable dtguest = ds.Tables.Count > 0 ? ds.Tables[1] : new DataTable();
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                       hotelreceipt.IsError = false;
                       hotelreceipt.WID = Convert.ToInt32(dt.Rows[0]["_WID"]);
                       hotelreceipt.SupportEmail = dt.Rows[0]["_EmailIDSupport"] is DBNull ? string.Empty : dt.Rows[0]["_EmailIDSupport"].ToString();
                       hotelreceipt.PhoneNoSupport = dt.Rows[0]["_PhoneNoSupport"] is DBNull ? string.Empty : dt.Rows[0]["_PhoneNoSupport"].ToString();
                       hotelreceipt.MobileSupport = dt.Rows[0]["_MobileSupport"] is DBNull ? string.Empty : dt.Rows[0]["_MobileSupport"].ToString();
                       hotelreceipt.Email = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                       hotelreceipt.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                       hotelreceipt.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                       hotelreceipt.CompanyName = dt.Rows[0]["_CompanyName"] is DBNull ? string.Empty : dt.Rows[0]["_CompanyName"].ToString();
                       hotelreceipt.CompanyMobile = dt.Rows[0]["_CompanyMobile"] is DBNull ? string.Empty : dt.Rows[0]["_CompanyMobile"].ToString();
                        hotelreceipt.CompanyAddress = dt.Rows[0]["_CompanyAddress"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_CompanyAddress"]);
                        hotelreceipt.CompanyEmail = dt.Rows[0]["_EmailIDSupport"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_EmailIDSupport"]);
                        //Hotel Booking Details

                        hotelreceipt.IsError = false;
                        hotelreceipt.Msg = "Success";
                        hotelreceipt.Status = "Booked";
                        hotelreceipt.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? "" : dt.Rows[0]["_TransactionID"].ToString();
                        hotelreceipt.Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Amount"]);
                        hotelreceipt.HotelName = dt.Rows[0]["_HotelName"] is DBNull ? "" : dt.Rows[0]["_HotelName"].ToString();
                        hotelreceipt.NoOfRooms = dt.Rows[0]["_NoOfRooms"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_NoOfRooms"]);
                        hotelreceipt.NoOfChilds = dt.Rows[0]["_Childs"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Childs"]);
                        hotelreceipt.NoOfAdults = dt.Rows[0]["_Adults"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Adults"]);
                        hotelreceipt.DestinationName = dt.Rows[0]["_DestinationName"] is DBNull ? "" : dt.Rows[0]["_DestinationName"].ToString();
                        hotelreceipt.CheckInDate = dt.Rows[0]["_CheckInDate"] is DBNull ? "" : dt.Rows[0]["_CheckInDate"].ToString();
                        hotelreceipt.CheckOutDate = dt.Rows[0]["_CheckOutDate"] is DBNull ? "" : dt.Rows[0]["_CheckOutDate"].ToString();
                        hotelreceipt.TranDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString();
                        hotelreceipt.BookingId = dt.Rows[0]["_BookingId"] is DBNull ? "" : dt.Rows[0]["_BookingId"].ToString();
                        hotelreceipt.ConfirmationNo = dt.Rows[0]["_ConfirmationNo"] is DBNull ? "" : dt.Rows[0]["_ConfirmationNo"].ToString();
                       
                        //Hotel Booking Guest Details
                        if (dtguest.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtguest.Rows)
                            {
                                var hrg = new HotelReceiptGuest()
                                {
                                    Lead = dr["_Lead"] is DBNull ? "" : dr["_Lead"].ToString(),
                                    Child = dr["_Child"] is DBNull ? "" : dr["_Child"].ToString(),
                                    Title = dr["_Title"] is DBNull ? "" : dr["_Title"].ToString(),
                                    Name = dr["_Name"] is DBNull ? "" : dr["_Name"].ToString(),
                                    MobileNo = dr["_MobileNo"] is DBNull ? "" : dr["_MobileNo"].ToString(),
                                    EmailID = dr["_EmailID"] is DBNull ? "" : dr["_EmailID"].ToString(),
                                    Age = dr["_Age"] is DBNull ? 0 : Convert.ToInt32(dr["_Age"])
                                };
                                HotelReceiptGuest.Add(hrg);
                            }
                            hotelreceipt.HotelReceiptGuest = HotelReceiptGuest;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return hotelreceipt;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }
}
