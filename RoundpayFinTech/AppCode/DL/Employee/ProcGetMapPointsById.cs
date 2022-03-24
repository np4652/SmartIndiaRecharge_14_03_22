using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetMapPointsById : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetMapPointsById(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                //new SqlParameter("@LoginID", _req.LoginID),
                //new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@Id", _req.CommonInt)
            };
            var _alist = new List<MapPointsModel>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow dr in dt.Rows)
                {
                    _alist.Add(new MapPointsModel
                    {
                        //Statuscode = ErrorCodes.One,
                        //Msg = "Success",
                        //ServiceID = Convert.ToInt32(dr["_ServiceID"]),
                        //OPTypeID = Convert.ToInt32(dr["_OPTypeID"]),
                        Description = Convert.ToString(dr["Description"]),
                        Latitude = Convert.ToString(dr["_Latitute"]),
                        Longitude = Convert.ToString(dr["_Longitute"])
                    });
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
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "ProcGetMapPointsById";
    }
}
