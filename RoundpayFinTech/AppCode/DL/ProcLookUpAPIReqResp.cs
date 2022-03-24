using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Lookup;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLookUpAPIReqResp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcLookUpAPIReqResp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (LookUpDBLogReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@MobileNo", _req.Mobile??string.Empty),
                new SqlParameter("@CurrentOperator", _req.CurrentOperator??string.Empty),
                new SqlParameter("@CurrentCircle", _req.CurrentCircle??string.Empty),
                new SqlParameter("@Response", _req.Response??string.Empty),
                new SqlParameter("@Request", _req.Request??string.Empty),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@APIType", _req.APIType),
                new SqlParameter("@IsCircleOnly", _req.IsCircleOnly)
            };


            var _res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.CommonInt = dt.Rows[0]["OID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["OID"]);
                        _res.CommonInt2 = dt.Rows[0]["CircleID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["CircleID"]);
                        _res.CommonStr2 = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_Operator"]);
                        _res.Status = dt.Rows[0]["_OpGroupID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OpGroupID"]);
                        _res.CommonBool = dt.Rows[0]["_IsCircleOnly"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCircleOnly"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _res.Msg = ex.Message;
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_LookUpAPIReqResp";
    }
}
