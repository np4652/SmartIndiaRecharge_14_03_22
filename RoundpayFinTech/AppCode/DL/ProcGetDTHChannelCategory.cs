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
    public class ProcGetDTHChannelCategory : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDTHChannelCategory(IDAL dal) => _dal = dal;
        
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID", _req.CommonInt)
            };
            var _List = new List<DTHChannelCategory>();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    var _Category = new DTHChannelCategory
                    {
                        ID = Convert.ToInt32(dr["_ID"]),
                       Category=Convert.ToString(dr["_CategoryName"])
                    };
                    _List.Add(_Category);
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

        public string GetName() => "select * from Master_ChannelCategory";
    }
}
