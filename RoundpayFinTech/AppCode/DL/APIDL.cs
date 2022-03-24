using Fintech.AppCode.DB;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class APIDL
    {
        private readonly IDAL _dal;
        public APIDL(IDAL dal)
        {
            _dal = dal;
        }       
        public bool ChangeAPIStatus(int ID)
        {
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@ID", ID);
            string query = "update tbl_API set _ActiveSts=_ActiveSts^1 where _ID=@ID";
            try
            {
                _dal.Execute(query, param);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

       
        

        public async Task DownAPI(int OID,int APIID) {
            const string query = "update tbl_APISwitching set _IsActive=0 where _OID=@OID and _APIID=@APIID";
            try
            {
                SqlParameter[] param = new SqlParameter[2];
                param[0] = new SqlParameter("@OID", OID);
                param[1] = new SqlParameter("@APIID", APIID);
                await _dal.ExecuteAsync(query,param);
            }
            catch (Exception)
            {
            }
        }        
    }
}
