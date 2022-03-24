using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateAPIDenominationUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateAPIDenominationUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (APIDenominationReq)obj;
            if (_req.Action == false)
            {
                _req.IsDnomActive = false;
                _req.IsDRangeActive = false;
            }
            else
            {
                _req.IsDnomActive = _req.DenomID == 0 ? false : true;
                _req.IsDRangeActive = _req.DRangeID == 0 ? false : true;
            }
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.OID),
                new SqlParameter("@APIID", _req.APIId),
                new SqlParameter("@DenomID", _req.DenomID),
                new SqlParameter("@DRangeID", _req.DRangeID),
                new SqlParameter("@IP", _req.CommonStr),
                new SqlParameter("@Browser", _req.CommonStr2),
                new SqlParameter("@IsDenomActive", _req.IsDnomActive),
                new SqlParameter("@IsDRangeActive", _req.IsDRangeActive),
                new SqlParameter("@MaxCountD",_req.DenomID == 0 ? 0 : _req.MaxCount),
                new SqlParameter("@MaxCountDR",_req.DRangeID == 0 ? 0 : _req.MaxCount),
                new SqlParameter("@UserID",_req.UserID),
                new SqlParameter("@CircleID",_req.CircleID)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateAPIDenominationUser";
    }

    public class ProcRemoveAPIDenominationUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRemoveAPIDenominationUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@ID", _req.CommonInt)
            };

            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_RemoveAPIDenominationUser";
    }

    public class ProcRemoveAPIDenomination : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRemoveAPIDenomination(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@ID", _req.CommonInt)
            };

            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_RemoveAPIDenomination";
    }
}