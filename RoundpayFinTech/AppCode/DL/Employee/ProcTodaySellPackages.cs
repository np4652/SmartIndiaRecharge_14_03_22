using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcTodaySellPackages : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTodaySellPackages(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param =
             {
              new SqlParameter("@LoginID", LoginID)
            };
            var Userslists = new List<UserPackageDetail>();
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Userslists.Add(new UserPackageDetail
                        {
                            UserName=Convert.ToString(dr["_Name"]),
                            OutletName=Convert.ToString(dr["_OutletName"]),
                            PackageName=Convert.ToString(dr["_PackageName"])
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
                    LoginTypeID = 0,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Userslists;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => @"Declare @DT Date=GetDate()
                                     IF OBJECT_ID('tempdb.dbo.#tempChild', 'U') IS NOT NULL  DROP TABLE #tempChild; 
                                     select _ID UserID  into #tempChild from tbl_Users(nolock) where _EmpID=@LoginID
                                     select u._Name,u._OutletName,m._Package _PAckageName,p.* from tbl_Users_Package p(nolock) inner join #tempChild c(nolock) on c.UserID=p._UserID
                                                                                                                               inner join tbl_Users u(nolock) on u._ID=c.UserID
                                                                                                                               Inner join MASTER_PACKAGE m (nolock) On m._ID=p._PackageID 
                                     where Cast(P._ModifyDate as Date)=@DT ";
    }
}
