using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCallbackType : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCallbackType(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new List<CallbackTypeModel>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), new SqlParameter[] { new SqlParameter("@LoginID", (int)obj) });
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt16(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new CallbackTypeModel
                            {
                                ID = item["_ID"] is DBNull?0:Convert.ToInt32(item["_ID"]),
                                Type = item["_Type"] is DBNull ? "" : item["_Type"].ToString(),
                                Parameters = item["_PARAMETERS"] is DBNull ? "" : item["_PARAMETERS"].ToString(),
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
        public string GetName() => "Proc_GetCallbackType";
    }
}
