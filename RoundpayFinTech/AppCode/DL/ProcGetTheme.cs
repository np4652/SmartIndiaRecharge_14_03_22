using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTheme : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetTheme(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            int WID = (int)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@WID",WID)
            };
            var res = new List<Theme>();
            try
            {
                var dt = _dal.Get(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new Theme
                        {
                            Id = Convert.ToInt32(dr["_ID"]),
                            ThemeName = Convert.ToString(dr["_ThemeName"]),
                            IsCurrentlyActive=Convert.ToBoolean(dr["_IsCurrentlyActive"]),
                            IsWLAllowed=Convert.ToBoolean(dr["_IsWLAllowed"]),
                            IsOnlyForAdmin = Convert.ToBoolean(dr["_IsOnlyForAdmin"]),
                        });
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => @"select distinct t._ID,  _ThemeName , 	isnull(t._IsActive,0) _IsActive	, isnull(_IsOnlyForAdmin,0) _IsOnlyForAdmin,	isnull(_IsWLAllowed,0) _IsWLAllowed ,IIF(ISNULL(w._ThemeID,0)>0,1,0) _IsCurrentlyActive  from MASTER_Theme t Left join MASTER_WEBSITE w on t._ID=w._ThemeID and w._ID=@WID 
          where t._IsActive=1 and ((ISNULL(_IsOnlyForAdmin,0)=0 and _IsWLAllowed=1) or ( @WID=1))";
    }
}
