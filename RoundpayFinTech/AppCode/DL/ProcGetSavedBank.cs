using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSavedBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSavedBank(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            var _res = new List<Bank>();
            SqlParameter[] param =
            {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@UserID", _req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _res.Add(new Bank
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                        BankID = Convert.ToInt32(dt.Rows[i]["_BankID"]),
                        BankName = dt.Rows[i]["_Bank"].ToString(),
                        BranchName = dt.Rows[i]["_BranchName"].ToString(),
                        AccountHolder = dt.Rows[i]["_AccountHolder"].ToString(),
                        AccountNo = dt.Rows[i]["_AccountNo"].ToString(),
                        IFSCCode = dt.Rows[i]["_IFSCCode"].ToString(),
                        Charge = Convert.ToDecimal(dt.Rows[i]["_Charge"]),
                        Logo = dt.Rows[i]["_Logo"].ToString(),
                        CID = dt.Rows[i]["CID"].ToString(),
                        ISQRENABLE = dt.Rows[i]["IsQREnable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["IsQREnable"]),
                        RImageUrl = Convert.ToString(dt.Rows[i]["_RImageUrl"]),
                        QRPath = Convert.ToString(dt.Rows[i]["_RImageUrl"]),
                        UPINUmber = Convert.ToString(dt.Rows[i]["_UPINumber"]),
                        Remark = Convert.ToString(dt.Rows[i]["_Remark"]),
                        IsShow = dt.Rows[i]["_IsShow"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsShow"]),
                        AccParty = dt.Rows[i]["_AccPartyID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_AccPartyID"])

                    });
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
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetSavedBank";
    }
    public class ProcUpdateBankShow : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateBankShow(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (int)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {


                 //new SqlParameter("@LT",req.LoginTypeID),
                 //new SqlParameter("@LoginID",req.LoginID),
                 new SqlParameter("@Id",req)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    //LoginTypeID = req.LoginTypeID,
                    ///  UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_Update_Isbankshow";
    }
}

