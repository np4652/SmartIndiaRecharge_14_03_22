using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAreaForPC : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAreaForPC(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int _UID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _UID)
            };
            var resp = new List<ASAreaMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        resp.Add(new ASAreaMaster
                        {
                            AreaID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            Area = row["_Area"] is DBNull ? string.Empty : row["_Area"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? string.Empty : row["_EntryDate"].ToString(),
                            ModifyDate = row["_ModifiedDate"] is DBNull ? "NA" : row["_ModifiedDate"].ToString()
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
                    LoginTypeID = 1,
                    UserId = _UID
                });
            }
            return resp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_GetAreaMaster";
        }

        public bool CheckECollectionSts(int UID)
        {
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@ID", UID);
            string query = "select isnull(_IsECollection,0) from tbl_Users where _ID=@ID";
            try
            {
                var dt = _dal.Get(query, param);
                return dt.Rows[0][0] is DBNull ? false : Convert.ToBoolean(dt.Rows[0][0]);
            }
            catch (Exception)
            {
            }
            return false;
        }

    }
}
