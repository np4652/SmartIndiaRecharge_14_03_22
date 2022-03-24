using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    
    public class ProcGetWhatsappSenderNo : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhatsappSenderNo(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@apiID",req.CommonInt)
            };
            List<WhatsappAPIDetail> res = new List<WhatsappAPIDetail> { };
            DataTable dt = _dal.GetByProcedure(GetName(), param);
            if (dt.Rows.Count > 0)
            {
                    foreach (DataRow row in dt.Rows)
                    {
                        var WhatsappAPIDetail = new WhatsappAPIDetail
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Mobileno = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            WID = row["_WID"] is DBNull ? 0 : Convert.ToInt32(row["_WID"]),
                            APIID = row["_APIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIID"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            DEPID = row["_DeptID"] is DBNull ? "0" : Convert.ToString(row["_DeptID"]),
                            APICode = row["_ApiCode"] is DBNull ? "" : Convert.ToString(row["_ApiCode"])
                        };
                            res.Add(WhatsappAPIDetail);
                 
                    }
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetWhatsappSenderNo";
    }
}
