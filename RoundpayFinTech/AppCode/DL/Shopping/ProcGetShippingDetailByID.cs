using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetShippingDetailByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetShippingDetailByID(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@LT", _req.LoginTypeID),
                 new SqlParameter("@ID", _req.CommonInt)
        };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    int i = 0;
                    ShoppingShipping OrderReport = new ShoppingShipping()
                    {
                        id = dt.Rows[0]["_id"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_id"]),
                        CustomerName = dt.Rows[0]["_CustomerName"].ToString() is DBNull ? "" : dt.Rows[0]["_CustomerName"].ToString(),
                        Address = dt.Rows[0]["_Address"].ToString() is DBNull ? "" : dt.Rows[0]["_Address"].ToString(),
                        Area = dt.Rows[0]["_Area"].ToString() is DBNull ? "" : dt.Rows[0]["_Area"].ToString(),
                        LandMark = dt.Rows[0]["_LandMark"].ToString() is DBNull ? "" : dt.Rows[0]["_LandMark"].ToString(),
                        MobileNo = dt.Rows[0]["_MobileNo"].ToString() is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString(),
                        CityName = dt.Rows[0]["_City"].ToString() is DBNull ? "" : dt.Rows[0]["_City"].ToString(),
                        StateName = dt.Rows[0]["StateName"].ToString() is DBNull ? "" : dt.Rows[0]["StateName"].ToString(),
                        Pin = dt.Rows[0]["_Pin"].ToString() is DBNull ? "" : dt.Rows[0]["_Pin"].ToString(),
                        Title = dt.Rows[0]["_Title"].ToString() is DBNull ? "" : dt.Rows[0]["_Title"].ToString(),
                        CityId = dt.Rows[0]["_CityID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CityID"]),
                        StateId = dt.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_StateID"])

                    };
                    return OrderReport;
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return new OrderReport();
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_GetShippingDetailByID";
        }

    }
}
