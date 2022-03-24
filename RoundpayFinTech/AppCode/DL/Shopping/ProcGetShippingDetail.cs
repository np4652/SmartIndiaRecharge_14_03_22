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

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetShippingDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetShippingDetail(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@LT", _req.LoginTypeID),
                 new SqlParameter("@OrderID", _req.CommonInt)
        };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    int i = 0;
                    ShoppingShipping OrderReport = new ShoppingShipping()
                    {
                        id= dt.Rows[i]["_id"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_id"]),
                        CustomerName = dt.Rows[i]["_CustomerName"].ToString() is DBNull ? "" : dt.Rows[i]["_CustomerName"].ToString(),
                        Address = dt.Rows[i]["_Address"].ToString() is DBNull ? "" : dt.Rows[i]["_Address"].ToString(),
                        Area = dt.Rows[i]["_Area"].ToString() is DBNull ? "" : dt.Rows[i]["_Area"].ToString(),
                        LandMark = dt.Rows[i]["_LandMark"].ToString() is DBNull ? "" : dt.Rows[i]["_LandMark"].ToString(),
                        MobileNo = dt.Rows[i]["_MobileNo"].ToString() is DBNull ? "" : dt.Rows[i]["_MobileNo"].ToString(),
                        CityName = dt.Rows[i]["_City"].ToString() is DBNull ? "" : dt.Rows[i]["_City"].ToString(),
                        StateName = dt.Rows[i]["StateName"].ToString() is DBNull ? "" : dt.Rows[i]["StateName"].ToString(),
                        Pin = dt.Rows[i]["_Pin"].ToString() is DBNull ? "" : dt.Rows[i]["_Pin"].ToString(),
                        Title = dt.Rows[i]["_Title"].ToString() is DBNull ? "" : dt.Rows[i]["_Title"].ToString(),

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
            return "Proc_ShippingDetail";
        }

    }
}
