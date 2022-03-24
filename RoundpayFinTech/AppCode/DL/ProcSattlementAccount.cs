using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSattlementAccountList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSattlementAccountList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.UserID)
            };
            var res = new List<SattlementAccountModels>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        var data = new SattlementAccountModels
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            BankName = row["_BankName"] is DBNull ? null : Convert.ToString(row["_BankName"]),
                            IFSC = row["_IFSC"] is DBNull ? null : Convert.ToString(row["_IFSC"]),
                            AccountNumber = row["_AccountNumber"] is DBNull ? null : Convert.ToString(row["_AccountNumber"]),
                            AccountHolder = row["_AccountHolder"] is DBNull ? null : Convert.ToString(row["_AccountHolder"]),
                            EntryBy = row["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(row["_EntryBy"]),
                            EntryDate = row["_EntryDate"] is DBNull ? null : Convert.ToString(row["_EntryDate"]),
                            ApprovedBY = row["_ApprovedBY"] is DBNull ? null : Convert.ToString(row["_ApprovedBY"]),
                            ApprovalIp = row["_ApprovalIp"] is DBNull ? null : Convert.ToString(row["_ApprovalIp"]),
                            ApprovalDate = row["_ApprovalDate"] is DBNull ? null : Convert.ToString(row["_ApprovalDate"]),
                            Actualname = row["_Actualname"] is DBNull ? null : Convert.ToString(row["_Actualname"]),
                            UTR = row["_UTR"] is DBNull ? null : Convert.ToString(row["_UTR"]),
                            APIID = row["_APIID"] is DBNull ? null : Convert.ToString(row["_APIID"]),
                            ApprovalStatus = row["_ApprovalStatus"] is DBNull ? 0 : Convert.ToInt32(row["_ApprovalStatus"]),
                            VerificationStatus = row["_VerificationStatus"] is DBNull ? 0 : Convert.ToInt32(row["_VerificationStatus"]),
                            IsDefault = row["_IsDefault"] is DBNull ? false : Convert.ToBoolean(row["_IsDefault"]),
                            VerificationText = row["_VerificationText"] is DBNull ? null : Convert.ToString(row["_VerificationText"]),
                            ApprovalText = row["_ApprovalText"] is DBNull ? null : Convert.ToString(row["_ApprovalText"]),

                        };
                        res.Add(data);
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetSettlementAccountList";
    }
    public class ProcGetSettlementAccountbyID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSettlementAccountbyID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt)
            };
            var res = new SattlementAccountModels();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                    res.BankID = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BankID"]);
                    res.BankName = dt.Rows[0]["_BankName"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_BankName"]);
                    res.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_IFSC"]);
                    res.AccountNumber = dt.Rows[0]["_AccountNumber"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AccountNumber"]);
                    res.AccountHolder = dt.Rows[0]["_AccountHolder"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AccountHolder"]);
                    res.EntryBy = dt.Rows[0]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EntryBy"]);
                    res.EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_EntryDate"]);
                    res.ApprovedBY = dt.Rows[0]["_ApprovedBY"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ApprovedBY"]);
                    res.ApprovalIp = dt.Rows[0]["_ApprovalIp"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ApprovalIp"]);
                    res.ApprovalDate = dt.Rows[0]["_ApprovalDate"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ApprovalDate"]);
                    res.Actualname = dt.Rows[0]["_Actualname"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_Actualname"]);
                    res.UTR = dt.Rows[0]["_UTR"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_UTR"]);
                    res.APIID = dt.Rows[0]["_APIID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_APIID"]);
                    res.ApprovalStatus = dt.Rows[0]["_ApprovalStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ApprovalStatus"]);
                    res.VerificationStatus = dt.Rows[0]["_VerificationStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VerificationStatus"]);
                    res.IsDefault = dt.Rows[0]["_IsDefault"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDefault"]);
                    res.VerificationText = dt.Rows[0]["_VerificationText"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_VerificationText"]);
                    res.ApprovalText = dt.Rows[0]["_ApprovalText"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_ApprovalText"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetSettlementAccountbyID";
    }
    public class ProcUpdateUTRByAccountUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateUTRByAccountUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt),
                new SqlParameter("@UTR",req.CommonStr??string.Empty),
                new SqlParameter("@IP",req.CommonStr2??string.Empty),
                new SqlParameter("@Browser",req.CommonStr3??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UpdateUTRByAccountUser";
    }
    public class ProcUpdateSattlementAccount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSattlementAccount(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateSattlementAccount";
        public object Call(object obj)
        {
            var req = (SattlementAccountModels)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",LoginType.ApplicationUser),
                new SqlParameter("@LoginID",req.EntryBy),
                 new SqlParameter("@ID",req.ID is null ? 0:req.ID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@BankName",req.BankName),
                new SqlParameter("@IFSC",req.IFSC),
                new SqlParameter("@AccountNumber",req.AccountNumber),
                new SqlParameter("@AccountHolder",req.AccountHolder),
                new SqlParameter("@BankID",req.BankID),
                 new SqlParameter("@ApprovalStatus",req.ApprovalStatus)
                
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();



    }
    public class ProcSetDefaultSattlementAccount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSetDefaultSattlementAccount(IDAL dal) => _dal = dal;
        public string GetName() => "proc_SetDefaultSattlementAccount";
        public object Call(object obj)
        {
            var req = (SattlementAccountModels)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",LoginType.ApplicationUser),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
               new SqlParameter("@UserID",req.UserID),


            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();



    }
    public class ProcDeleteSattlementAccount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeleteSattlementAccount(IDAL dal) => _dal = dal;
        public string GetName() => "proc_DeleteSattlementAccount";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt),
               new SqlParameter("@UserID",req.UserID),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();



    }
    public class ProcApproveBankDetails : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcApproveBankDetails(IDAL dal) => _dal = dal;
        public string GetName() => "proc_AcceptOrRejectBankDetails";
        public async Task<object> Call(object obj)
        {
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            var _req = (GetEditUser)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@RequestID", _req.RequestID),
                new SqlParameter("@RequestStatus", _req.RequestStatus),
            };
            try
            {

                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        Task<object> IProcedureAsync.Call()
        {
            throw new NotImplementedException();
        }
    }

    public class ProcGetSattlementAccountAstatus : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetSattlementAccountAstatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<_Status>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@FilterID", req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new _Status
                        {
                            ID = Convert.ToInt32(dr["ApprovalID"]),
                            Name = Convert.ToString(dr["_Name"])

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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => @"select * from MASTER_ApprovalStatus";
    }

    public class ProcGetSattlementAccountVstatus : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetSattlementAccountVstatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new List<_Status>();
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@FilterID", req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new _Status
                        {
                            ID = Convert.ToInt32(dr["_VerificationID"]),
                            Name = Convert.ToString(dr["_Name"])

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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => @"select * from MASTER_VerificationStatus";
    }
}
