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
    public class ProcGetSettlementReport: IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetSettlementReport(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            WalletRequest _req = (WalletRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginId", _req.LoginID),
                new SqlParameter("@TransMode", _req.CommonStr??string.Empty),
                new SqlParameter("@Status", _req.CommonInt),
                new SqlParameter("@TopRows", _req.CommonInt2),
                new SqlParameter("@ID", _req.ID),
                new SqlParameter("@MobileNo", _req.Mobile??string.Empty),
                new SqlParameter("@TransactionId", _req.TransactionId??string.Empty),
                new SqlParameter("@GroupID", _req.GroupID),
                new SqlParameter("@ApproveDate", _req.ApprovalDate),
                new SqlParameter("@OID", _req.CommonInt3)
            };
            List<WalletRequest> _lst = new List<WalletRequest>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var _res = new WalletRequest
                        {
                            ID =dr["_ID"] is DBNull?0: Convert.ToInt32(dr["_ID"]),
                            Amount = dr["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Amount"]),
                            Status = dr["_Status"] is DBNull ? 0 : Convert.ToInt32(dr["_Status"]),
                            Remark = dr["_ApprovedRemark"] is DBNull ? string.Empty : Convert.ToString(dr["_ApprovedRemark"]),
                            TransMode = dr["_TransactionMode"] is DBNull ? string.Empty : Convert.ToString(dr["_TransactionMode"]),
                            Charge = dr["_Charges"] is DBNull ? 0 : Convert.ToDecimal(dr["_Charges"]),
                            UserId = dr["_UserId"] is DBNull ? 0 : Convert.ToInt32(dr["_UserId"]),
                            UserRoleId = dr["_UserRoleId"] is DBNull ? string.Empty : Convert.ToString(dr["_UserRoleId"]),
                            UserName = dr["_Name"] is DBNull ? string.Empty : Convert.ToString(dr["_Name"]),
                            Mobile = dr["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(dr["_MobileNo"]),
                            BankName = dr["_BankName"] is DBNull ? string.Empty : Convert.ToString(dr["_BankName"]),
                            IFSC = dr["_IFSC"] is DBNull ? string.Empty : Convert.ToString(dr["_IFSC"]),
                            AccountHolder = dr["_AccountHolder"] is DBNull ? string.Empty : Convert.ToString(dr["_AccountHolder"]),
                            AccountNumber = dr["_AccountNumber"] is DBNull ? string.Empty : Convert.ToString(dr["_AccountNumber"]),
                            EntryDate = dr["_EntryDate"] is DBNull ? string.Empty : Convert.ToString(dr["_EntryDate"]),
                            TransactionId = dr["_TransactionId"] is DBNull ? string.Empty : Convert.ToString(dr["_TransactionId"]),
                            TID = dr["_TID"] is DBNull ? string.Empty : Convert.ToString(dr["_TID"]),
                            Address = dr["_Address"] is DBNull ? string.Empty : Convert.ToString(dr["_Address"]),
                            City = dr["_City"] is DBNull ? string.Empty : Convert.ToString(dr["_City"]),
                            State = dr["StateName"] is DBNull ? string.Empty : Convert.ToString(dr["StateName"]),
                            Email = dr["_EmailID"] is DBNull ? string.Empty : Convert.ToString(dr["_EmailID"]),
                            Pincode = dr["_Pincode"] is DBNull ? string.Empty : Convert.ToString(dr["_Pincode"]),
                            ApprovalDate = dr["_ApproveDate"] is DBNull ? string.Empty : Convert.ToString(dr["_ApproveDate"]),
                            BankRRN = dr["_BankRRN"] is DBNull ? string.Empty : Convert.ToString(dr["_BankRRN"]),
                            MiniBankBalance= dr["_BBalance"] is DBNull ? 0 : Convert.ToDecimal(dr["_BBalance"]),
                            MiniBankCapping = dr["_BCapping"] is DBNull ? 0 : Convert.ToDecimal(dr["_BCapping"]),
                            InRealTime = dr["_InRealTime"] is DBNull ? false : Convert.ToBoolean(dr["_InRealTime"])
                        };

                        if (_req.ID > 0)
                        {
                            return _res;
                        }
                        else
                        {
                            _lst.Add(_res);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _lst;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetSettlementReport";
    }
}
