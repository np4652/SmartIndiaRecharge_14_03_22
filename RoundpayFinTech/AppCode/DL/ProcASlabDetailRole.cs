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
    public class ProcASlabDetailRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcASlabDetailRole(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.CommonInt),
                new SqlParameter("@RoleID",req.CommonInt2),
                new SqlParameter("@OID",req.CommonInt3),
            };

            var DetailList = new List<ASlabDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ASlabDetail
                        {
                            VendorID = Convert.ToInt32(dr["_VendorID"]),
                            VendorName = Convert.ToString(dr["_VendorName"]),
                            OID = dr["_CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["_CategoryID"]),
                            AmtType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]),
                            CommType = dr["_CommType"] is DBNull ? 0 : Convert.ToInt32(dr["_CommType"]),
                            CommAmount = dr["_CommAmount"] is DBNull ? 0 : Convert.ToDecimal(dr["_CommAmount"]),
                            ModifyDate = Convert.ToString(dr["_ModifyDate"])
                        };
                        DetailList.Add(data);
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
            var res = new ASlabDetailModel
            {
                SlabID = req.CommonInt,
                RoleID = req.CommonInt2,
                OID = req.CommonInt3,
                CommissitionDetail = DetailList,
            };
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ASlabDetailRole";
    }
}
