using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDeleteSlab : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcDeleteSlab(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            SqlParameter[] param = {
               new SqlParameter("@loginId", req.LoginID),               
               new SqlParameter("@Id", req.CommonInt),               
            };
            try
            {
                var dt = await _dal.GetAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0]["statuscode"] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0]["statuscode"]);
                    res.Msg = dt.Rows[0]["msg"] is DBNull ? ErrorCodes.TempError : Convert.ToString(dt.Rows[0]["msg"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ErrorCodes.UNSUCCESS;
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => @"if (select count(1) from MASTER_SLAB(nolock) where _ID=@Id and _EntryBy=@loginId) = 0
                                     begin
                                     	select -1 statuscode,'Either Slab may not exists or you may not permitted to delete it' msg
                                     	return
                                     end
                                     if(select count(1) from tbl_Users(nolock) where _SlabId = @Id) > 0
                                     begin
                                     	select -1 statuscode,'Since it is assigned to some users, you cannot delete it' msg
                                     	return
                                     end
                                     Update Master_Slab set _IsDeleted=1 where _ID=@ID and _EntryBy=@loginId;
                                     select 1 StatusCode,'Slab deleted succesfully' msg";
    }
}