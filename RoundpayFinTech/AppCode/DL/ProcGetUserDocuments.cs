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
    public class ProcGetUserDocuments : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserDocuments(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (DocTypeMaster)obj;

            var list = new List<DocTypeMaster>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginId),
                new SqlParameter("@UserId", req.UserId),
                new SqlParameter("@OutletIDApproval", req.OutletID)
            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var item = new DocTypeMaster
                            {
                                StatusCode = Convert.ToInt16(dt.Rows[0][0]),
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"] is DBNull ? 0 : dt.Rows[i]["_ID"]),
                                DocTypeID = Convert.ToInt16(dt.Rows[i]["DocTypeID"] is DBNull ? 0 : dt.Rows[i]["DocTypeID"]),
                                DocName = dt.Rows[i]["_DocName"] is DBNull ? "" : dt.Rows[i]["_DocName"].ToString(),
                                DocUrl = dt.Rows[i]["_DocURL"] is DBNull ? "" : dt.Rows[i]["_DocURL"].ToString(),
                                VerifyStatus = dt.Rows[i]["_IsVerified"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_IsVerified"].ToString()),
                                EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "Not Yet" : dt.Rows[i]["_EntryDate"].ToString(),
                                ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "Not Yet" : dt.Rows[i]["_ModifyDate"].ToString(),
                                Remark = dt.Rows[i]["_Remark"] is DBNull ? "" : dt.Rows[i]["_Remark"].ToString(),
                                UserId = Convert.ToInt32(dt.Rows[i]["_UserId"] is DBNull ? 0 : dt.Rows[i]["_UserId"]),
                                OutletID = Convert.ToInt32(dt.Rows[i]["OutletID"] is DBNull ? 0 : dt.Rows[i]["OutletID"]),
                                KYCStatus = Convert.ToInt16(dt.Rows[i]["KYCStatus"] is DBNull ? 0 : dt.Rows[i]["KYCStatus"]),
                            };
                            item.IsOptional = Convert.ToBoolean(dt.Rows[i]["_IsOptional"] is DBNull ? false : dt.Rows[i]["_IsOptional"]);
                            item.DRemark = dt.Rows[i]["_DRemark"] is DBNull ? "" : dt.Rows[i]["_DRemark"].ToString();
                            list.Add(item);
                        }
                    }
                    else
                    {
                        list.Add(new DocTypeMaster
                        {
                            StatusCode = ErrorCodes.Minus1,
                            Msg = dt.Rows[0]["Msg"].ToString()
                        });
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
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return list;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserDocuments";
    }

    public class ProcGetUserDocumentsList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserDocumentsList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (DocTypeMaster)obj;

            var list = new List<DocTypeMaster>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginId),
                new SqlParameter("@UserId", req.UserId)
            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new DocTypeMaster
                        {
                            StatusCode = Convert.ToInt32(dt.Rows[0][0]),
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"] is DBNull ? 0 : dt.Rows[i]["_ID"]),
                            UserId = Convert.ToInt32(dt.Rows[i]["_UserId"] is DBNull ? 0 : dt.Rows[i]["_UserId"]),
                            OutletID = Convert.ToInt32(dt.Rows[i]["_OutletID"] is DBNull ? 0 : dt.Rows[i]["_OutletID"]),
                            DocTypeID = Convert.ToInt16(dt.Rows[i]["_DocTypeID"] is DBNull ? 0 : dt.Rows[i]["_DocTypeID"]),
                            DocName = dt.Rows[i]["_DocName"] is DBNull ? "" : dt.Rows[i]["_DocName"].ToString(),
                            VerifyStatus = dt.Rows[i]["_IsVerified"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_IsVerified"]),
                            DRemark = dt.Rows[i]["_TYPE"] is DBNull ? "" : Convert.ToString(dt.Rows[i]["_TYPE"]),
                            Remark = dt.Rows[i]["_Remark"] is DBNull ? "" : dt.Rows[i]["_Remark"].ToString()
                        };
                        list.Add(item);
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
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return list;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserDocumentsList";
    }
}