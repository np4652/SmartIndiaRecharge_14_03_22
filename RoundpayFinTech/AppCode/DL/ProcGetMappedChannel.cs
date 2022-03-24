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
    public class ProcGetMappedChannel : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMappedChannel(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@PackageID", _req.CommonInt)
            };
            var _List = new List<DTHChannel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    var _channel = new DTHChannel
                    {
                        ID = Convert.ToInt32(dr["_ID"]),
                        ChannelName=Convert.ToString(dr["_Chanel"]),
                        CategoryID=Convert.ToInt32(dr["_CategoryID"]),
                        IsActive=Convert.ToBoolean(dr["_IsActive"]),
                    };
                    _List.Add(_channel);
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

        public string GetName() => "proc_GetMappedChannel";
    }
}
