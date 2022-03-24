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
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class procReferSetting : IProcedure
    {

        private readonly IDAL _dal;
        public procReferSetting(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetMasterTopupCommission";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),


            };
            var res = new List<Master_Topup_Commission>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var master_topup_commission = new Master_Topup_Commission
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Comm = row["_Comm"] is DBNull ? "" : Convert.ToString(row["_Comm"]),
                            CommOnReg = row["_CommOnReg"] is DBNull ? "" : Convert.ToString(row["_CommOnReg"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            IsGreaterThan = row["_IsGreaterThan"] is DBNull ? "" : Convert.ToString(row["_IsGreaterThan"]),
                            TopUpName = row["_TopUpName"] is DBNull ? "" : Convert.ToString(row["_TopUpName"])
                        };
                        res.Add(master_topup_commission);
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
    }
    public class procMasterRole : IProcedure
    {

        private readonly IDAL _dal;
        public procMasterRole(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetMasterRole";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),


            };
            var res = new List<Master_Role>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var master_role = new Master_Role
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Role = row["_Role"] is DBNull ? "" : Convert.ToString(row["_Role"]),
                            SignupAmount = row["_SignupAmount"] is DBNull ? "0" : Convert.ToString(row["_SignupAmount"])
                        };
                        res.Add(master_role);
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
    }


    public class ProcUpdate_Topup_Commission : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdate_Topup_Commission(IDAL dal) => _dal = dal;
        public string GetName() => "procUpdateTopupCommission";
        public object Call(object obj)
        {
            var req = (Master_Topup_Commission)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            //   @IsGreaterThan int= null, @Comm int= null, @IsActive bit = null,@CommOnReg  int= null
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@IsGreaterThan",req.IsGreaterThan),
                new SqlParameter("@Comm",req.Comm),
                new SqlParameter("@IsActive",req.IsActive),
                //new SqlParameter("@CommOnReg",req.CommOnReg),
                  new SqlParameter("@IP",req.Ip),
                new SqlParameter("@Browser",req.Browser)




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
    public class ProcUpdateMaster_Role : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateMaster_Role(IDAL dal) => _dal = dal;
        public string GetName() => "ProcUpdateMasterRole";
        public object Call(object obj)
        {
            var req = (Master_Role)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            //   @IsGreaterThan int= null, @Comm int= null, @IsActive bit = null,@CommOnReg  int= null
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@SignupAmount",req.SignupAmount),
                new SqlParameter("@IP",req.Ip),
                new SqlParameter("@Browser",req.Browser)

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
    public class ProcReferralCommission : IProcedure
    {

        private readonly IDAL _dal;
        public ProcReferralCommission(IDAL dal) => _dal = dal;
        public string GetName() => "select * from MASTER_TOPUP_COMMISSION";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;            
            var res = new List<ReferralCommission>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var referral_commission = new ReferralCommission
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            TopUpName = row["_TopUpName"] is DBNull ? "" : Convert.ToString(row["_TopUpName"]),
                            IsGreaterThan = row["_IsGreaterThan"] is DBNull ? 0 : Convert.ToInt32(row["_IsGreaterThan"]),
                            Comm = row["_Comm"] is DBNull ? 0 : Convert.ToInt32(row["_Comm"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            CommOnReg = row["_CommOnReg"] is DBNull ? 0 : Convert.ToInt32(row["_CommOnReg"])                            
                        };
                        res.Add(referral_commission);
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
            }
            return res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
    }
    public class ProcUpdate_referral_Commission : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdate_referral_Commission(IDAL dal) => _dal = dal;
        public string GetName() => "procUpdateReferralCommission";
        public object Call(object obj)
        {
            var req = (ReferralCommission)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@CommOnReg",req.CommOnReg)
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
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
    }
}
