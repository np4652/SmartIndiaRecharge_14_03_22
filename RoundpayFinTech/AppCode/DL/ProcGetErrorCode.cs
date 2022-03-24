using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetErrorCode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetErrorCode(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@EID",req.CommonInt),
                new SqlParameter("@ECode",req.CommonStr??""),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new List<ErrorCodeDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if ((req.CommonStr??"") == "" && req.CommonInt == 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var errorCodeDetail = new ErrorCodeDetail
                            {
                                EID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Error = row["_Error"] is DBNull ? "" : row["_Error"].ToString(),
                                Code = row["_Code"] is DBNull ? "" : row["_Code"].ToString(),
                                Status = row["_Status"] is DBNull ? 0 : Convert.ToInt32(row["_Status"]),
                                ErrType = row["_Type"] is DBNull ? 0 : Convert.ToInt32(row["_Type"]),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                                _ErrType = row["_ErrType"] is DBNull ? "" : row["_ErrType"].ToString(),
                                IsDown= row["_IsDown"] is DBNull ? false : Convert.ToBoolean(row["_IsDown"]),
                                IsResend = row["_IsResend"] is DBNull ? false : Convert.ToBoolean(row["_IsResend"])
                            };
                            errorCodeDetail.ErrorWithCode = "["+ errorCodeDetail._ErrType + "](" + errorCodeDetail.Code + ") " + errorCodeDetail.Error;
                            res.Add(errorCodeDetail);
                        }
                    }
                    else
                    {
                        var errorCodeDetail= new ErrorCodeDetail
                        {
                            EID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            Error = dt.Rows[0]["_Error"] is DBNull ? "" : dt.Rows[0]["_Error"].ToString(),
                            Code = dt.Rows[0]["_Code"] is DBNull ? "" : dt.Rows[0]["_Code"].ToString(),
                            Status = dt.Rows[0]["_Status"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Status"]),
                            ErrType = dt.Rows[0]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Type"]),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            _ErrType = dt.Rows[0]["_ErrType"] is DBNull ? "" : dt.Rows[0]["_ErrType"].ToString(),
                            IsDown = dt.Rows[0]["_IsDown"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDown"]),
                            IsResend = dt.Rows[0]["_IsResend"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsResend"])
                        };
                        errorCodeDetail.ErrorWithCode = "[" + errorCodeDetail._ErrType + "](" + errorCodeDetail.Code + ") " + errorCodeDetail.Error;
                        return errorCodeDetail;
                    }
                }
            }
            catch (Exception)
            {
            }
            if (req.CommonInt == 0 && (req.CommonStr ?? "") == "")
                return res;
            else
                return new ErrorCodeDetail();
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetErrorCode";
        public IEnumerable<ErrorTypeMaster> ErTypeMasters()
        {
            var errorTypeMasters = new List<ErrorTypeMaster>();
            var dt = _dal.Get("select * from MASTER_ERROR_CODE order by _ErrorType");
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var erType = new ErrorTypeMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            ErrorType = dt.Rows[i]["_ErrorType"].ToString(),
                            Remark = dt.Rows[i]["_Remark"].ToString()
                        };
                        errorTypeMasters.Add(erType);
                    }
                }
                catch (Exception)
                {
                }
            }
            return errorTypeMasters;
        }
    }
}
