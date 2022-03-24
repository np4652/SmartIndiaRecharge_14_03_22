using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcValidataRechargeApirequest : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcValidataRechargeApirequest(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (ValidataRechargeApirequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Token", _req.Token ?? ""),
                new SqlParameter("@IP", _req.IPAddress ?? ""),
                new SqlParameter("@SPKey", _req.SPKey ?? "SPKEY"),
                new SqlParameter("@OID", _req.OID)
            };

            var _res = new ValidataRechargeApiResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError,
                ErrorCode=ErrorCodes.Unknown_Error.ToString(),
                OpParams=new System.Collections.Generic.List<OperatorParams>()
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0] is DBNull ? "-1" : dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    _res.ErrorCode = dt.Rows[0]["ErrorCode"] is DBNull ? "0" : dt.Rows[0]["ErrorCode"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.OID = Convert.ToInt32(dt.Rows[0]["OID"] is DBNull ? "0" : dt.Rows[0]["OID"]);
                        _res.OpType = Convert.ToInt32(dt.Rows[0]["OPType"] is DBNull ? "0" : dt.Rows[0]["OPType"]);
                        _res.CircleValidationType = Convert.ToInt32(dt.Rows[0]["CircleValidationType"] is DBNull ? "0" : dt.Rows[0]["CircleValidationType"]);
                        _res.LookupAPIID = Convert.ToInt32(dt.Rows[0]["LookupAPIID"] is DBNull ? 0 : dt.Rows[0]["LookupAPIID"]);
                        _res.LookupReqID = dt.Rows[0]["LookupReqID"] is DBNull ? "" : dt.Rows[0]["LookupReqID"].ToString();
                        _res.IsBBPS = dt.Rows[0]["IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsBBPS"]);
                        _res.IsBilling = dt.Rows[0]["IsBilling"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsBilling"]);
                        _res.SPKey = dt.Rows[0]["_SPKey"] is DBNull ? "" : dt.Rows[0]["_SPKey"].ToString();
                        _res.Operator = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : dt.Rows[0]["_Operator"].ToString();
                        _res.MobileU = dt.Rows[0]["_MobileU"] is DBNull ? string.Empty : dt.Rows[0]["_MobileU"].ToString();
                        _res.DBGeoCode = dt.Rows[0]["_DBGeoCode"] is DBNull ? string.Empty : dt.Rows[0]["_DBGeoCode"].ToString();
                        _res.OpGroupID = dt.Rows[0]["_OpGroupID"] is DBNull ? 0 :Convert.ToInt32(dt.Rows[0]["_OpGroupID"]);
                        _res.SCode = dt.Rows[0]["_SCode"] is DBNull ? string.Empty :dt.Rows[0]["_SCode"].ToString();
                    }
                }
                if (ds.Tables.Count > 1)
                {
                    var dtOpParams = ds.Tables[1];
                    foreach (DataRow item in dtOpParams.Rows)
                    {
                        _res.OpParams.Add(new OperatorParams
                        {
                            DataType = item["_DataType"] is DBNull ? string.Empty : item["_DataType"].ToString(),
                            Ind = item["_Ind"] is DBNull ? 0 : Convert.ToInt32(item["_Ind"]),
                            IsAccountNo = item["_IsAccountNo"] is DBNull ? false : Convert.ToBoolean(item["_IsAccountNo"]),
                            IsOptional = item["_IsOptional"] is DBNull ? false : Convert.ToBoolean(item["_IsOptional"]),
                            MaxLength = item["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(item["_MaxLength"]),
                            MinLength = item["_MinLength"] is DBNull ? 0 : Convert.ToInt32(item["_MinLength"]),
                            Param = item["_ParamName"] is DBNull ? string.Empty : item["_ParamName"].ToString(),
                            RegEx = item["_RegEx"] is DBNull ? string.Empty : item["_RegEx"].ToString(),
                            Remark = item["_Remark"] is DBNull ? string.Empty : item["_Remark"].ToString()
                        });
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_ValidataRechargeApirequest";
    }
}
