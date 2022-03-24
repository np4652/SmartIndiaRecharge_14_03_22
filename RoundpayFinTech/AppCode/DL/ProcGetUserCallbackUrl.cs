using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserCallbackUrl : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserCallbackUrl(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _res = new List<UserCallBackModel>();
            var _req = (UserCallBackModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.UserID),
                new SqlParameter("@CallBackType", _req.CallbackType)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new UserCallBackModel
                        {
                            CallbackType = Convert.ToInt16(dt.Rows[i]["_TypeId"].ToString()),
                            CallBackName = dt.Rows[i]["_Type"].ToString(),
                            Remark = dt.Rows[i]["_REMARK"].ToString(),
                            URL = dt.Rows[i]["_URL"].ToString(),
                            UpdateUrl = dt.Rows[i]["_UpdateURL"] is DBNull ? string.Empty : dt.Rows[i]["_UpdateURL"].ToString(),
                            UserID = Convert.ToInt32(dt.Rows[i]["_USERID"].ToString())
                        };
                        _res.Add(item);
                    }
                }
            }
            catch (Exception ex)
            { }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetUserCallbackUrl";
    }
}
