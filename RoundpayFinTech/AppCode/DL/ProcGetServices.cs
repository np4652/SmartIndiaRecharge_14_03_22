using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetServices : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetServices(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            var _ListServices = new List<ServiceMaster>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var read = new ServiceMaster
                        {
                            ServiceID = Convert.ToInt32(dr["_ID"]),
                            ServiceName = dr["_Name"].ToString(),
                            SCode = dr["_SCode"].ToString(),
                            WalletTypeID = Convert.ToInt32(dr["_WalletTypeID"]),
                            IsVisible = dr["_IsVisible"] is DBNull ? false : Convert.ToBoolean(dr["_IsVisible"]),
                        };
                        _ListServices.Add(read);
                    }
                }
            }
            catch (Exception)
            {
            }

            return _ListServices;
        }

        public string GetName() => "select _ID,_Name,_SCode,_WalletTypeID,_IsVisible from MASTER_SERVICE where _IsDeleted=0 and _inSlab=1 and _IsActive=1 order by _Ind";
        public int GetServiceID(int ID) {
            SqlParameter[] param = { 
             new SqlParameter("@ID",ID)
            };
            try
            {
                var dt = _dal.Get(QueryForServiceID,param);
                if (dt.Rows.Count > 0) {
                    return dt.Rows[0]["_ServiceTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ServiceTypeID"]);
                }
            }
            catch (Exception ex)
            {
            }
            return 0;
        }
        private string QueryForServiceID = "select _ServiceTypeID from MASTER_OPTYPE where _ID=@ID";
    }
    
    public class ProcGetUsedServices : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetUsedServices(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID)
            };

            var _ListServices = new List<ServiceMaster>();
            try
            {
                DataSet ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var read = new ServiceMaster
                        {
                            ServiceID = Convert.ToInt32(dr["_ID"]),
                            ServiceName = dr["_Name"].ToString()
                        };

                        _ListServices.Add(read);
                    }
                }
            }
            catch (Exception)
            {
            }

            return _ListServices;
        }

        public async Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetUsedServiceList";
    }

    public class ProcGetIsKYCRequired : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetIsKYCRequired(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var SPKey = (string)obj;
            SqlParameter[] param = {
                new SqlParameter("@SPKey",SPKey??string.Empty)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["_IsKYCRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsKYCRequired"]);
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "select top 1 s._IsKYCRequired from tbl_Operator o inner join MASTER_OPTYPE mo on mo._ID=o._Type inner join MASTER_SERVICE s on s._ID=mo._ServiceTypeID where o._OPID=@SPKey";
    }
}
