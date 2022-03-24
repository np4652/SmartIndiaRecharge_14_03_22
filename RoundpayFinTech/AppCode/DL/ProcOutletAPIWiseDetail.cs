using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcOutletAPIWiseDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcOutletAPIWiseDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                  new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OutletID", _req.CommonInt)
            };
            var _alist = new List<ApiWiseDetail>();
            try
            {
                var dt =  _dal.GetByProcedure(GetName(), param);
                if (!dt.Columns.Contains("Msg"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new ApiWiseDetail
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            OutletID = Convert.ToInt32(dt.Rows[i]["_OutletID"]),
                            APIID = Convert.ToInt32(dt.Rows[i]["_APIID"]),
                            APIName = dt.Rows[i]["APIName"].ToString(),
                            APIOutletId = dt.Rows[i]["_APIOutletID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[i]["_APIOutletID"]),
                            VerifyStatus = Convert.ToInt32(dt.Rows[i]["_VerifyStatus"]),
                            _VerifyStatus = dt.Rows[i]["VerifyStatus"].ToString(),
                            BBPSID = dt.Rows[i]["_BBPSID"] is DBNull ? "" : dt.Rows[i]["_BBPSID"].ToString(),
                            BBPSStatus = Convert.ToInt32(dt.Rows[i]["_BBPSStatus"]),
                            _BBPSStatus = dt.Rows[i]["BBPSStatus"].ToString(),
                            AEPSID = dt.Rows[i]["_AEPSID"] is DBNull ? "" : dt.Rows[i]["_AEPSID"].ToString(),
                            AEPSStatus = dt.Rows[i]["_AEPSStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_AEPSStatus"]),
                            _AEPSStatus = dt.Rows[i]["AEPSStatus"].ToString(),
                            DMTID = dt.Rows[i]["_DMTID"] is DBNull ? "" : dt.Rows[i]["_DMTID"].ToString(),
                            DMTStatus = Convert.ToInt32(dt.Rows[i]["_DMTStatus"]),
                            _DMTStatus = Convert.ToString(dt.Rows[i]["DMTStatus"]),
                            PSAID = dt.Rows[i]["_PSAID"] is DBNull ? "" : dt.Rows[i]["_PSAID"].ToString(),
                            PSAStatus = Convert.ToInt32(dt.Rows[i]["_PSAStatus"]),
                            _PSAStatus = Convert.ToString(dt.Rows[i]["PSAStatus"]),
                            LastUpdatedOn = dt.Rows[i]["LastUpdatedOn"].ToString()
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return _alist;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_outletAPIWiseDetail";

    }
}
