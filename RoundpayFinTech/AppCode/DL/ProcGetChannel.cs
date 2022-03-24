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
    public class ProcGetChannel : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetChannel(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@ID", _req.CommonInt)
            };
            var _List = new List<DTHChannel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var _Package = new DTHChannel
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                        ChannelName = Convert.ToString(dt.Rows[i]["_Chanel"]),
                        CategoryID = Convert.ToInt32(dt.Rows[i]["_CategoryID"]),
                        CategoryName = Convert.ToString(dt.Rows[i]["_CategoryName"]),
                    };
                    _List.Add(_Package);
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

        public string GetName() => "proc_GetChannel";
    }
}
