using System;
using System.Data.SqlClient;
using System.Text;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcAddShippingAddress : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAddShippingAddress(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (SAddress)obj;
            var res = new ShippingAddress();

            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@Id",req.ID),
                new SqlParameter("@CustomerName",req.CustomerName),
                new SqlParameter("@Title",req.Title??""),
                new SqlParameter("@Address",req.Address),
                new SqlParameter("@PIN",req.PIN),
                new SqlParameter("@CityID",req.CityID),
                new SqlParameter("@StateID",req.StateID),
                new SqlParameter("@Landmark",req.Landmark??""),
                new SqlParameter("@MobileNo",req.MobileNo),
                new SqlParameter("@Area",req.Area),
                new SqlParameter("@IsDefault",req.IsDefault),
                new SqlParameter("@IsDelete",req.IsDeleted),
                new SqlParameter("@lat",req.Latitude ?? ""),
                new SqlParameter("@long",req.Longitude ?? "")
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0][1]);
                    if (res.Statuscode == 1 && req.IsDeleted == false)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{Address} ,City : {City},PIN : {PIN},Mobile : {Mobile},<br/>LandMark : {LandMark}");
                        sb.Replace("{Address}", Convert.ToString(dt.Rows[0]["_Address"]));
                        sb.Replace("{City}", Convert.ToString(dt.Rows[0]["_City"]));
                        sb.Replace("{PIN}", Convert.ToString(dt.Rows[0]["_PIN"]));
                        sb.Replace("{Mobile}", Convert.ToString(dt.Rows[0]["_MobileNo"]));
                        sb.Replace("{LandMark}", Convert.ToString(dt.Rows[0]["_LandMark"]));
                        res.ID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        res.Address = sb.ToString();
                        res.Latitude = dt.Rows[0]["_Lat"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Lat"]);
                        res.Longitude = dt.Rows[0]["_Long"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Long"]);
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
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_AddShippingAddress";
    }
}