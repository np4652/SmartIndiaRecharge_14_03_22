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
    public class ProcUserReport : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserReport(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UserReport";
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@IsViewed", _req.CommonBool),
                new SqlParameter("@UserMobile", _req.CommonStr),
                new SqlParameter("@UserID", _req.CommonInt),
                new SqlParameter("@IntroducerID", _req.CommonInt2),
                new SqlParameter("@IntroducerMobile", _req.CommonStr2),
                new SqlParameter("@FromDate", _req.CommonStr3 ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.CommonStr4 ?? DateTime.Now.ToString("dd MMM yyyy")),
            };
            var _List = new List<Users>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    var _user = new Users
                    {
                        ID=Convert.ToInt32(dr["_ID"]),
                        UserName=Convert.ToString(dr["_Name"]),
                        RoleID = Convert.ToInt32(dr["_RoleID"]),
                        RoleName = Convert.ToString(dr["_Role"]),
                        OutletName = Convert.ToString(dr["_OutletName"]),
                        MobileNo = Convert.ToString(dr["_MobileNo"]),
                        Email = Convert.ToString(dr["_EmailID"]),
                        PinCode = Convert.ToInt32(dr["_PinCode"]),
                        ReferalID = Convert.ToInt32(dr["_ReferalID"]),
                        IntroducerName = Convert.ToString(dr["_IName"]),
                        IntroducerMobile = Convert.ToString(dr["_IMobileNo"]),
                        EntryDate = Convert.ToString(dr["_EntryDate"]),
                        ModifyDate = Convert.ToString(dr["_ModifyDate"]),
                        IsViewed = Convert.ToBoolean(dr["_IsViewed"]),
                        Balance = Convert.ToDecimal(dr["_Balance"])
                    };
                    _List.Add(_user);
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
            return _List;
        }

        public object Call() => throw new NotImplementedException();
    }

    public class ProcUpdateViewStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateViewStatus(IDAL dal) => _dal = dal;
        public string GetName() => "update tbl_Users set _IsViewed=1 where _ID=@userID;select 1,'successfully marked as viewed' Msg";
        public object Call(object obj)
        {
            var _req = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@userID", _req),
            };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
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
                    //UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();
    }

    public class ProcCountPendingViews : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCountPendingViews(IDAL dal) => _dal = dal;
        public string GetName() => "select 1,Count(1) as _Count from tbl_Users where ISNULL(_IsViewed,0)=0";
        public object Call(object obj) => throw new NotImplementedException();


        public object Call() {
            SqlParameter[] param = {
            };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["_Count"].ToString();
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
                    //UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        } 
    }
}

