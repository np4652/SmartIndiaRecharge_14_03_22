using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserByDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserByDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new UserInfo();
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UT",req.CommonInt),
                new SqlParameter("@ID",req.CommonInt2),
                new SqlParameter("@MobileNo",req.CommonStr??""),
            };
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    var dt2 = ds.Tables.Count == 2 ? ds.Tables[1] : new DataTable();
                    if (dt.Rows.Count > 0)
                    {
                        if (!dt.Columns.Contains("Msg"))
                        {
                            res.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                            res.OutletName = dt.Rows[0]["_OutletName"] is DBNull ? "" : dt.Rows[0]["_OutletName"].ToString();
                            res.UserID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                            res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                            res.RoleID = dt.Rows[0]["_RoleID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_RoleID"]);
                            res.IsDenominationSwtichBlock = dt.Rows[0]["_IsDenominationSwtichBlock"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDenominationSwtichBlock"]);
                        }
                    }
                    if (dt2.Rows.Count > 0)
                    {
                        res.blockDetails = new List<UserBlockDetail>();
                        foreach (DataRow item in dt2.Rows)
                        {
                            res.blockDetails.Add(new UserBlockDetail
                            {
                                SwitchingTypeID= item["_SwitchingTypeID"] is DBNull?0:Convert.ToInt16(item["_SwitchingTypeID"]),
                                IsActive = item["_IsActive"] is DBNull ? false : Convert.ToBoolean(item["_IsActive"]),
                            });
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserByDetail";
    }
}
