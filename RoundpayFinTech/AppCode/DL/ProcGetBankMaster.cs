using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBankMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBankMaster(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@ID", _req.CommonInt)
            };
            var bankMasters = new List<BankMaster>();
            var bankMaster = new BankMaster();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (_req.CommonInt > 0)
                    {
                        bankMaster = new BankMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[0]["_ID"]),
                            BankName = dt.Rows[0]["_Bank"].ToString(),
                            AccountLimit = Convert.ToInt32(dt.Rows[0]["_ACNo_Limit"] is DBNull ? 0 : dt.Rows[0]["_ACNo_Limit"]),
                            Code = dt.Rows[0]["_Code"] is DBNull ? string.Empty : dt.Rows[0]["_Code"].ToString(),
                            IsIMPS = dt.Rows[0]["_IsIMPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsIMPS"]),
                            IsNEFT = dt.Rows[0]["_IsNEFT"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsNEFT"]),
                            IsACVerification = Convert.ToBoolean(dt.Rows[0]["_IsACVerification"] is DBNull ? false : dt.Rows[0]["_IsACVerification"]),
                            IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : dt.Rows[0]["_IFSC"].ToString(),
                            Logo = dt.Rows[0]["_Logo"] is DBNull ? string.Empty : dt.Rows[0]["_Logo"].ToString(),
                            EKO_BankID = dt.Rows[0]["_EKO_BankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_EKO_BankID"]),
                            Mahagram_BankID = dt.Rows[0]["_MagramBankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_MagramBankID"]),
                            IIN = dt.Rows[0]["_IIN"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_IIN"]),
                            ISAEPSStatus = dt.Rows[0]["_AEPSStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_AEPSStatus"]),
                            BankType = dt.Rows[0]["_BankType"] is DBNull ? "" : dt.Rows[0]["_BankType"].ToString(),
                            BAVENVEBankID = dt.Rows[0]["BAVENVEBankID"] is DBNull ? "" : dt.Rows[0]["BAVENVEBankID"].ToString(),
                            RDaddyBankID = dt.Rows[0]["_RDaddyBankID"] is DBNull ? 0 :Convert.ToInt32(dt.Rows[0]["_RDaddyBankID"]),
                            Pay1MoneyBankID = dt.Rows[0]["_Pay1MoneyBankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Pay1MoneyBankID"])
                        };
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var bankM = new BankMaster
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                BankName = dt.Rows[i]["_Bank"].ToString(),
                                AccountLimit = Convert.ToInt32(dt.Rows[i]["_ACNo_Limit"] is DBNull ? 0 : dt.Rows[i]["_ACNo_Limit"]),
                                Code = dt.Rows[i]["_Code"] is DBNull ? string.Empty : dt.Rows[i]["_Code"].ToString(),
                                IsIMPS = dt.Rows[i]["_IsIMPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsIMPS"]),
                                IsNEFT = dt.Rows[i]["_IsNEFT"] is DBNull ? false: Convert.ToBoolean(dt.Rows[i]["_IsNEFT"]),
                                IsACVerification = Convert.ToBoolean(dt.Rows[i]["_IsACVerification"] is DBNull ? false : dt.Rows[i]["_IsACVerification"]),
                                IFSC = dt.Rows[i]["_IFSC"] is DBNull ? string.Empty : dt.Rows[i]["_IFSC"].ToString(),
                                Logo = dt.Rows[i]["_Logo"] is DBNull ? string.Empty : dt.Rows[i]["_Logo"].ToString(),
                                EKO_BankID = dt.Rows[i]["_EKO_BankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_EKO_BankID"]),
                                Mahagram_BankID = dt.Rows[i]["_MagramBankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_MagramBankID"]),
                                IIN = dt.Rows[i]["_IIN"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_IIN"]),
                                ISAEPSStatus = dt.Rows[i]["_AEPSStatus"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_AEPSStatus"]),
                                BankType = dt.Rows[i]["_BankType"] is DBNull ? "" : dt.Rows[i]["_BankType"].ToString(),
                                BAVENVEBankID = dt.Rows[i]["BAVENVEBankID"] is DBNull ? "" : dt.Rows[i]["BAVENVEBankID"].ToString(),
                                RDaddyBankID = dt.Rows[i]["_RDaddyBankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_RDaddyBankID"]),
                                Pay1MoneyBankID = dt.Rows[0]["_Pay1MoneyBankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Pay1MoneyBankID"])
                            };
                            bankMasters.Add(bankM);
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (_req.CommonInt > 0)
                return bankMaster;
            return bankMasters;
        }

        public object Call()
        {
            string Query = "select _ID,_Bank,_IIN,_IFSC,_IsPopular from MASTER_BANK where isnull(_IIN,'')<>'' and  _AEPSStatus=1 order by _IsPopular desc, _Bank";
            var bankMasters = new List<BankMaster>();
            try
            {
                DataTable dt = _dal.Get(Query);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        bankMasters.Add(new BankMaster
                        {
                            ID = dt.Rows[i]["_ID"] is DBNull?0: Convert.ToInt32(dt.Rows[i]["_ID"]),
                            BankName = dt.Rows[i]["_Bank"]is DBNull?string.Empty: dt.Rows[i]["_Bank"].ToString(),
                            IFSC = dt.Rows[i]["_IFSC"] is DBNull?string.Empty: dt.Rows[i]["_IFSC"].ToString(),
                            IIN = dt.Rows[i]["_IIN"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_IIN"])
                        });
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
            return bankMasters;
        }
        public object CallCC()
        {
            string Query = "select _ID,_Bank,_IIN,_IFSC,_IsPopular from MASTER_BANK where _IsCreditCard=1 order by _IsPopular desc, _Bank";
            var bankMasters = new List<BankMaster>();
            try
            {
                DataTable dt = _dal.Get(Query);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        bankMasters.Add(new BankMaster
                        {
                            ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"]),
                            BankName = dt.Rows[i]["_Bank"] is DBNull ? string.Empty : dt.Rows[i]["_Bank"].ToString(),
                            IFSC = dt.Rows[i]["_IFSC"] is DBNull ? string.Empty : dt.Rows[i]["_IFSC"].ToString(),
                            IIN = dt.Rows[i]["_IIN"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_IIN"])
                        });
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
            return bankMasters;
        }
        public string GetName() => "proc_GetBankMaster";
    }
}
