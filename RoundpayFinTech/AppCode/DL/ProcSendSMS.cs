using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSendSMS : IProcedure
    {
        protected readonly IDAL _dal;
        public ProcSendSMS(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SMSSendREQ _req = (SMSSendREQ)obj;
            SqlParameter[] param = {
                new SqlParameter("@FormatType", _req.FormatType),
                new SqlParameter("@MobileNo", _req.MobileNo ?? ""),
                new SqlParameter("@tp_ReplaceKeywords", _req.Tp_ReplaceKeywords),
                new SqlParameter("@GeneralSMS", _req.GeneralSMS ?? ""),
                new SqlParameter("@WID", _req.WID)
            };
            SMSSendResp _resp = new SMSSendResp();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.ResultCode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_resp.ResultCode == ErrorCodes.One)
                    {
                        _resp.SMSID = Convert.ToInt32(dt.Rows[0]["SMSID"]);
                        _resp.APIID = Convert.ToInt32(dt.Rows[0]["APIID"]);
                        _resp.SMS = dt.Rows[0]["SMS"].ToString();
                        _resp.SmsURL = dt.Rows[0]["SmsURL"].ToString();
                        _resp.APIMethod = dt.Rows[0]["APIMethod"].ToString();
                        _resp.TransactionID = dt.Rows[0]["TransactionID"].ToString();
                        _resp.MobileNo = dt.Rows[0]["MobileNo"].ToString();
                        _resp.IsLapu= dt.Rows[0]["IsLapu"] is DBNull?false: Convert.ToBoolean(dt.Rows[0]["IsLapu"]);
    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.WID,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SendSMS";
        }
    }

    public class ProcSendSMSUpdate : IProcedure
    {
        protected readonly IDAL _dal;
        public ProcSendSMSUpdate(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SMSUpdateREQ _req = (SMSUpdateREQ)obj;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("@SMSID", _req.SMSID);
            param[1] = new SqlParameter("@Status", _req.Status);
            param[2] = new SqlParameter("@Response", _req.Response ?? "");
            param[3] = new SqlParameter("@ResponseID", _req.ResponseID ?? "");
            ResponseStatus _resp = new ResponseStatus();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return _resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SendSMS_Update";
        }
    }
    public class ProcSendSMSBulk : IProcedure
    {
        protected readonly IDAL _dal;
        public ProcSendSMSBulk(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SMSREqSendBulk _req = (SMSREqSendBulk)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID", _req.ApiID),
                new SqlParameter("@SMS", _req.GeneralSMS),
                new SqlParameter("@IsLapu", _req.IsLapu),
                new SqlParameter("@WID", _req.WID),
                new SqlParameter("@bulkSms", _req.Tp_ReplaceKeywords),
        };
            ResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                _resp.Msg = ex.Message;
            }
            return _resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_InsertBulkSms";
        }
    }
}

