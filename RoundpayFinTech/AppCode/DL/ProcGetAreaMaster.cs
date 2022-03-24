using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAreaMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAreaMaster(IDAL dal)
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
    }
}
