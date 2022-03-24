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
  
    public class ProcGetOperatorParamByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOperatorParamByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var _resp = new List<OperatorParam>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@OID", req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var userList = new OperatorParam
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            OperatorName = dr["_OperatorName"] is DBNull ? string.Empty : dr["_OperatorName"].ToString(),
                            ParamName = dr["_ParamName"] is DBNull ? string.Empty : dr["_ParamName"].ToString(),
                            DataType = dr["_DataType"] is DBNull ? string.Empty : dr["_DataType"].ToString(),
                            Remark = dr["_Remark"] is DBNull ? string.Empty : dr["_Remark"].ToString(),
                            RegEx = dr["_RegEx"] is DBNull ? string.Empty : dr["_RegEx"].ToString(),
                            MinLength = dr["_MinLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MinLength"]),
                            MaxLength = dr["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MaxLength"]),
                            DropDown = dr["DropDown"] is DBNull ? string.Empty : dr["DropDown"].ToString()
                        };
                        _resp.Add(userList);
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetOperatorParamByID";
    }
}
