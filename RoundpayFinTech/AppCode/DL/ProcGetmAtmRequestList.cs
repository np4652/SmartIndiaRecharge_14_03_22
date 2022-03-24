using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetmAtmRequestList : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetmAtmRequestList(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetmAtmRequestList";
        public async Task<object> Call(object obj)
        {
            var _req = (MAtmFilterModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@mAtmStatus", _req.mAtamStatus),
                new SqlParameter("@mobile", _req.MobileNo ?? ""),
                new SqlParameter("@userId", _req.UserID == 0 ? 0:_req.UserID),
                new SqlParameter("@top", _req.TopRows == 0 ? 50 : _req.TopRows)
            };
            var _alist = new List<MAtmModel>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows[0][0].ToString() == "1")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new MAtmModel
                        {
                            StatusCode = ErrorCodes.One,
                            Msg = "Success",
                            ID = Convert.ToInt32(dt.Rows[i]["_id"]),
                            UserID = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                            Company = Convert.ToString(dt.Rows[i]["_Company"]),
                            MobileNo = Convert.ToString(dt.Rows[i]["_MobileNo"]),
                            EmailId = dt.Rows[i]["_EmailID"].ToString(),
                            RoleID = Convert.ToInt32(dt.Rows[i]["_RoleID"]),
                            RoleName = dt.Rows[i]["_Role"].ToString(),
                            OutletName = Convert.ToString(dt.Rows[i]["_OutletName"]),
                            PartnerName = Convert.ToString(dt.Rows[i]["PartnerName"]),
                            mAtamSerialNo = dt.Rows[i]["_MATMSerialNo"].ToString(),
                            mAtamStatus = Convert.ToInt32(dt.Rows[i]["_MATMStatus"]),
                            ExID = "FP" + Convert.ToString(dt.Rows[i]["_id"]),
                            ExName = Convert.ToString(dt.Rows[i]["_Name"]),
                            Address = Convert.ToString(dt.Rows[i]["_Address"]),
                            State = Convert.ToString(dt.Rows[i]["_State"]),
                            City = Convert.ToString(dt.Rows[i]["_City"]),
                            Pincode = Convert.ToString(dt.Rows[i]["_Pincode"]),
                            Pan = Convert.ToString(dt.Rows[i]["_PAN"]),
                            KYCDoc = "Pan card"
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception er)
            {

            }
            return _alist;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }
}
