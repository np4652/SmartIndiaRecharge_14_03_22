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
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    

    public class ProcInsertURTList : IProcedureAsync
    {
        private IDAL _dal;

        public ProcInsertURTList(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (UtrStatementUploadReq)obj;
           // var record = req.Record.Select(x => new { x.UserIdentity, x.UTR, x.Amount, x.VirtualAccount, x.CustomerCode, x.CustomerAccountNumber, x.Type, x.ProcName, x.Status }).ToList();
            DataTable dataTable =  req.Record.ToDataTable();
            dataTable.Columns.Remove("FileID");
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@IP", req.CommonStr),
                new SqlParameter("@Browser", req.CommonStr2),
                new SqlParameter("@AccountNo", req.CommonStr3),
                new SqlParameter("@BankID", req.CommonInt),
                new SqlParameter("@Record", dataTable)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_InsertUTRlist";

    }


    public class ProcgetUploadedUTRStatmentFiles : IProcedureAsync
    {
        private IDAL _dal;

        public ProcgetUploadedUTRStatmentFiles(IDAL dal) => _dal = dal;
        public Task<object> Call(object obj) => throw new NotImplementedException();

        public async Task<object> Call()
        {
            var resp = new List<UTRExcelMaster>();
            try
            {
                var dt = await _dal.GetAsync(GetName()).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new UTRExcelMaster
                        {
                            FileId = row["_Id"] is DBNull ? 0 : Convert.ToInt32(row["_Id"]),
                            FileName = row["_FileName"] is DBNull ? string.Empty : Convert.ToString(row["_FileName"]),
                            EntryDate = row["_EnrtyDate"] is DBNull ? string.Empty : Convert.ToString(row["_EnrtyDate"]),
                            IsJobDone = row["_IsJobDone"] is DBNull ? false : Convert.ToBoolean(row["_IsJobDone"]),
                        };
                        resp.Add(data);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }
        public string GetName() => @"select _ID,_FileName,convert(varchar,_EnrtyDate,106) as _EnrtyDate ,_IsJobDone from [dbo].[tbl_UTRStatementMaster](nolock)";
    }



    public class ProcgetUploadedUTRFilesDownLoad : IProcedureAsync
    {
        private IDAL _dal;
        public ProcgetUploadedUTRFilesDownLoad(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@FileId",req.CommonInt)
            };
            var resp = new List<UtrStatementUpload>();
            try
            {
                var dt = await _dal.GetAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new UtrStatementUpload
                        {
                            //FileId = row["_Id"] is DBNull ? 0 : Convert.ToInt32(row["_Id"])
                            //UserIdentity = row["_UserIdentity"] is DBNull ? string.Empty : Convert.ToString(row["_UserIdentity"]),
                            //UTR = row["_UTR"] is DBNull ? string.Empty : Convert.ToString(row["_UTR"]),
                            //Status = row["_Status"] is DBNull ? string.Empty : Convert.ToString(row["_Status"]),
                            //Amount = row["_Amount"] is DBNull ? 0 : Convert.ToDecimal(row["_Amount"]),
                        };
                        resp.Add(data);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => @"select * from [dbo].[tbl_UTRStatementDetail](nolock) where _FileId=@FileId";
    }



    public class ProcUtrStatementReconcile : IProcedureAsync
    {
        private IDAL _dal;

        public ProcUtrStatementReconcile(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (UtrStatementUploadReq)obj;
            
            SqlParameter[] param = {
                 new SqlParameter("@FiledId", req.CommonStr3),
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_Job_For_UTRStatement";

    }



    public class ProcUtrStatementDelete: IProcedureAsync
    {
        private IDAL _dal;

        public ProcUtrStatementDelete(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (UtrStatementUploadReq)obj;

            SqlParameter[] param = {

                 new SqlParameter("@FiledId", req.CommonStr3),
                 new SqlParameter("@LT", req.LoginTypeID),
                 new SqlParameter("@LoginID", req.LoginID)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_DeleteUtrStatement";

    }
}

