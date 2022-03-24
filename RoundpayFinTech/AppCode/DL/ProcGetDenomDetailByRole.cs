using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDenomDetailByRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenomDetailByRole(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetDenomDetailByRole";

        public object Call(object obj)
        {
            var req = (DenomDetailReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@DenomID", req.Detail.DenomID),
                new SqlParameter("@DenomRangeID", req.Detail.DenomRangeID),
                new SqlParameter("@SlabID", req.Detail.SlabID),
                 new SqlParameter("@OID", req.Detail.OID)

            };
            var _resList = new List<DenomDetailByRole>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        DenomDetailByRole read = new DenomDetailByRole();
                        read.ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]);
                        read.RoleID = dr["_RoleID"] is DBNull ? 0 : Convert.ToInt32(dr["_RoleID"]);
                        read.RoleName = dr["_RoleName"] is DBNull ? "" : Convert.ToString(dr["_RoleName"]);
                        read.DenomID = dr["_DenomID"] is DBNull ? 0 : Convert.ToInt32(dr["_DenomID"]);
                        read.DenomRangeID = dr["_DenomRangeID"] is DBNull ? 0 : Convert.ToInt32(dr["_DenomRangeID"]);
                        read.SlabID = dr["_SlabID"] is DBNull ? 0 : Convert.ToInt32(dr["_SlabID"]);
                        read.OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]);
                        read.AmtType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]);
                        read.Comm = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]);
                        read.IsAdminDefined = dr["_IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(dr["_IsAdminDefined"]);
                        read.ModifyDate = dr["_ModifyDate"] is DBNull ? "" : Convert.ToString(dr["_ModifyDate"]);
                        _resList.Add(read);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return _resList;
        }

        public object Call() => throw new NotImplementedException();
    }
}
