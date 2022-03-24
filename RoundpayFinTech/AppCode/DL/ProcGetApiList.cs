using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetApiList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetApiList(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonFilter _req = (CommonFilter)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginId", _req.LoginID)
            };
            var _res = new List<ApiListModel>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    for(int i = 0; i<dt.Rows.Count; i++)
                    {
                        //if (Convert.ToInt32(dt.Rows[i][0]) == ErrorCodes.One)
                        //{
                            _res.Add(new ApiListModel
                            {
                                Id = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                Name = Convert.ToString(dt.Rows[i]["_Name"])
                            });
                        //}
                    }
                }
            }
            catch(Exception ex) {
                
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "select _ID, _Name from tbl_API where _IsOutletRequired = 1";
        }
    }
}
