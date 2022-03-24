using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSMSAPICU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSMSAPICU(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (APIDetailReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@APIType",req.APIType),
                new SqlParameter("@Name",req.Name),
                new SqlParameter("@Url ",req.URL),
                new SqlParameter("@RequestMethod",req.RequestMethod),
                new SqlParameter("@ResponseTypeID",req.ResponseTypeID),
                new SqlParameter("@IP",req.IP),
                new SqlParameter("@Browser",req.Browser),
                new SqlParameter("@TransactionType",req.TransactionType),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@Default",req.Default),
                new SqlParameter("@IsWhatsApp",req.IsWhatsApp),
                new SqlParameter("@IsHangout",req.IsHangout),
                new SqlParameter("@IsTelegram",req.IsTelegram)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
       // public string GetName() => "proc_SMSAPI_CU";
        public string GetName() => "proc_UpdateSMSAPI";
    }

    public class ProcChangeAPIActiveStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangeAPIActiveStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {               
                new SqlParameter("@ID",req.CommonInt),
                new SqlParameter("@IsActive",req.CommonBool),
                new SqlParameter("@IsDefault",req.CommonBool1)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
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
        // public string GetName() => "proc_SMSAPI_CU";
        public string GetName() => "Update tbl_SMSAPI set _IsActive=@IsActive , _IsDefault = @IsDefault where _ID=@ID;select 1,'Change Status Successfully!' Msg";
    }
}