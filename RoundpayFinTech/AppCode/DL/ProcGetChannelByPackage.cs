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
    public class ProcGetChannelByPackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetChannelByPackage(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@PID", _req.CommonInt)
            };
            var _List = new List<DTHChannel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var channel = new DTHChannel
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ChannelID"]),
                        ChannelName = Convert.ToString(dt.Rows[i]["_ChannelName"]),
                        CategoryID= Convert.ToInt32(dt.Rows[i]["_CategoryID"]),
                        CategoryName = Convert.ToString(dt.Rows[i]["_CategoryName"])
                    };
                    _List.Add(channel);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _List;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetChannelByPackage";
        
    }
}
