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
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetOperatorOptional : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOperatorOptional(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@LT", _req.LoginTypeID),
                 new SqlParameter("@OID", _req.CommonInt)
        };
            var res = new OperatorParamModels
            {
                operatorOptionals = new List<OperatorOptional>(),
                operatorParams = new List<OperatorParam>(),
                OpOptionalDic=new List<OperatorOptionalDictionary>()
            };
            res.operatorOptionals = new List<OperatorOptional>();
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    res.operatorOptionals.Add(new OperatorOptional
                    {
                        OID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                        OptionalType = dr["_OptionalType"] is DBNull ? 0 : Convert.ToInt32(dr["_OptionalType"]),
                        DisplayName = dr["_DisplayName"] is DBNull ? string.Empty : dr["_DisplayName"].ToString(),
                        Remark = dr["_Remark"] is DBNull ? string.Empty : dr["_Remark"].ToString(),
                        IsList = dr["_IsList"] is DBNull ? false : Convert.ToBoolean(dr["_IsList"]),
                        IsMultiSelection = dr["_IsMultiSelection"] is DBNull ? false : Convert.ToBoolean(dr["_IsMultiSelection"])
                    });
                }
                if (ds.Tables.Count>=2)
                {
                    DataTable dt2 = ds.Tables[1];
                    foreach (DataRow dr in dt2.Rows)
                    {
                        res.operatorParams.Add(new OperatorParam
                        {
                            ParamName = dr["_ParamName"] is DBNull ? string.Empty : dr["_ParamName"].ToString(),
                            DataType = dr["_DataType"] is DBNull ? string.Empty : dr["_DataType"].ToString(),
                            MinLength = dr["_MinLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MinLength"]),
                            MaxLength = dr["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(dr["_MaxLength"]),
                            Ind = dr["_Ind"] is DBNull ? 0 : Convert.ToInt32(dr["_Ind"]),
                            RegEx = dr["_RegEx"] is DBNull ? string.Empty : dr["_RegEx"].ToString(),
                            Remark = dr["_Remark"] is DBNull ? string.Empty : dr["_Remark"].ToString(),
                            IsOptional = dr["_IsOptional"] is DBNull ? false : Convert.ToBoolean(dr["_IsOptional"]),
                            IsCustomerNo = dr["_IsCustomerNo"] is DBNull ? false : Convert.ToBoolean(dr["_IsCustomerNo"]),
                            IsDropDown = dr["_IsDropDown"] is DBNull ? false : Convert.ToBoolean(dr["_IsDropDown"]),
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"])
                        });
                    }
                    var dtOpParamsdtOpDic = ds.Tables.Count == 3 ? ds.Tables[2] : null;
                    if (dtOpParamsdtOpDic.Rows.Count > 0)
                    {
                        foreach (DataRow item in dtOpParamsdtOpDic.Rows)
                        {
                            res.OpOptionalDic.Add(new OperatorOptionalDictionary
                            {
                                OptionalID = item["_OptionalID"] is DBNull ? 0 : Convert.ToInt32(item["_OptionalID"]),
                                Value = item["_Value"] is DBNull ? string.Empty : item["_Value"].ToString()
                            });
                        }
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetOperatorOptional";
    }
}
