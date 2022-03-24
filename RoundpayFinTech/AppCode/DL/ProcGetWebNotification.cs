using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWebNotification : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetWebNotification(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var res = new List<WebNotification>();
            var req = (Fintech.AppCode.Model.CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@IsShowAll", req.CommonBool)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new WebNotification
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            UserID = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            Title = Convert.ToString(dr["_Title"]),
                            Notification = Convert.ToString(dr["_Notification"]),
                            IsSeen = dr["_IsSeen"] is DBNull ?false:Convert.ToBoolean(dr["_IsSeen"]),
                            EntryDate = Convert.ToString(dr["_EntryDate"]),
                            Img = Convert.ToString(dr["_Img"]),
                        };
                        res.Add(data);
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
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetWebNotification";
    }
}
