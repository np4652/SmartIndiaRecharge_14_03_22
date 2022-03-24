using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBankUpdateRequest : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBankUpdateRequest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SattlementAccountModels req = (SattlementAccountModels)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@Stype",req.CommonInt<0?0:req.CommonInt),
                new SqlParameter("@Text",req.CommonStr3),
                new SqlParameter("@Vstatus",req.VerificationStatus<0?null:req.VerificationStatus),
                new SqlParameter("@Astatus",req.ApprovalStatus<0?null:req.ApprovalStatus),
                new SqlParameter("@Top",req.CommonInt2)
            };
            List<SattlementAccountModels> resp = new List<SattlementAccountModels>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            SattlementAccountModels ll = new SattlementAccountModels
                            {
                                RequestID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                                MobileNo = row["_MobileNo"] is DBNull ? string.Empty : row["_MobileNo"].ToString(),
                                BankName = row["_BankName"] is DBNull ? string.Empty : row["_BankName"].ToString(),
                                IFSC = row["_IFSC"] is DBNull ? string.Empty : row["_IFSC"].ToString(),
                                AccountNumber = row["_AccountNumber"] is DBNull ? string.Empty : row["_AccountNumber"].ToString(),
                                AccountHolder = row["_AccountHolder"] is DBNull ? string.Empty : row["_AccountHolder"].ToString(),
                                Actualname = row["_Actualname"] is DBNull ? string.Empty : row["_Actualname"].ToString(),
                                Requestdate = row["_EntryDate"] is DBNull ? string.Empty : row["_EntryDate"].ToString(),
                                ApprovalStatus = row["_ApprovalStatus"] is DBNull ? 0 : Convert.ToInt32(row["_ApprovalStatus"]),
                                VerificationStatus = row["_VerificationStatus"] is DBNull ? 0 : Convert.ToInt32(row["_VerificationStatus"]),
                                ApprovalText = row["Astatus"] is DBNull ? string.Empty : Convert.ToString(row["Astatus"]),
                                VerificationText = row["Vstatus"] is DBNull ? string.Empty : Convert.ToString(row["Vstatus"])

                            };
                            resp.Add(ll);
                        }
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "Proc_GetBankUpdateRequest";
        }
    }
}
