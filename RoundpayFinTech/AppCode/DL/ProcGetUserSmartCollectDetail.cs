using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserSmartCollectDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserSmartCollectDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new UserSmartDetailModel
            {
                USDList = new List<UserSmartDetail>()
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.UserID)
            };
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.USDList.Add(new UserSmartDetail
                        {
                            SmartCollectTypeID = item["_SmartCollectTypeID"] is DBNull ? 0 : Convert.ToInt16(item["_SmartCollectTypeID"]),
                            SmartCollectType = item["_SmartCollectType"] is DBNull ? string.Empty : Convert.ToString(item["_SmartCollectType"]),
                            CustomerID = item["_CustomerID"] is DBNull ? string.Empty : Convert.ToString(item["_CustomerID"]),
                            SmartAccountNo = item["_SmartAccountNo"] is DBNull ? string.Empty : Convert.ToString(item["_SmartAccountNo"]),
                            SmartVPA = item["_SmartVPA"] is DBNull ? string.Empty : Convert.ToString(item["_SmartVPA"]),
                            SmartQRShortURL = item["_SmartQRShortURL"] is DBNull ? string.Empty : Convert.ToString(item["_SmartQRShortURL"]),
                            BankName = item["_BankName"] is DBNull ? string.Empty : Convert.ToString(item["_BankName"]),
                            IFSC = item["_IFSC"] is DBNull ? string.Empty : Convert.ToString(item["_IFSC"]),
                        });
                    }
                }
                if (ds.Tables.Count == 2)
                {
                    var dt2 = ds.Tables[1];
                    if (dt2.Rows.Count > 0)
                    {
                        res.Name = dt2.Rows[0]["_Name"] is DBNull ? string.Empty : dt2.Rows[0]["_Name"].ToString();
                        res.EmailID = dt2.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt2.Rows[0]["_EmailID"].ToString();
                        res.MobileNo = dt2.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt2.Rows[0]["_MobileNo"].ToString();
                        res.GSTIN = dt2.Rows[0]["_GSTIN"] is DBNull ? string.Empty : dt2.Rows[0]["_GSTIN"].ToString();
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetUserSmartCollectDetail";
    }
}
