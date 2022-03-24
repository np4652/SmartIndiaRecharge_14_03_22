
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
    public class ProcUpdateDownStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateDownStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (UpdateDownStatusReq)obj;
            var _resp = new UpdateDownStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            DataTable Tp_IDStatus = new DataTable();
            Tp_IDStatus.Columns.Add("ID", typeof(int));
            Tp_IDStatus.Columns.Add("Status", typeof(bool));
            try
            {
                for (int i = 0; i < _req.DataKVs.Count; i++)
                {
                    Tp_IDStatus.Rows.Add(new object[] { Convert.ToInt32(_req.DataKVs[i].Key), Convert.ToBoolean(_req.DataKVs[i].Value) });
                }
            }
            catch (Exception)
            {
                _resp.Msg = "Invalid Request to Update!";
                return _resp;
            }
            _req.Tp_IDStatus = Tp_IDStatus;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@tp_IDStatus", _req.Tp_IDStatus),
                new SqlParameter("@IP",_req.IP??string.Empty),
                new SqlParameter("@Browser",_req.Browser??string.Empty)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    int i = 0;
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[i][0]);
                    _resp.Msg = dt.Rows[i]["Msg"].ToString();
                    if (_resp.Statuscode == ErrorCodes.One)
                    {
                        _resp.UpOperators = dt.Rows[i]["UpOperators"].ToString();
                        _resp.DownOperators = dt.Rows[i]["DownOperators"].ToString();
                        _resp.UpMessage = dt.Rows[i]["UpMessage"].ToString();
                        _resp.DownMessage = dt.Rows[i]["DownMessage"].ToString();
                        _resp.SMSAPI = dt.Rows[i]["SMSAPI"].ToString();
                        _resp.SMSAPIID = Convert.ToInt32(dt.Rows[i]["SMSAPIID"] is DBNull ? 0 : dt.Rows[i]["SMSAPIID"]);
                        _resp.Company = Convert.ToString(dt.Rows[i]["Company"]);
                        _resp.CompanyAddress = Convert.ToString(dt.Rows[i]["CompanyAddress"]);
                        _resp.CompanyDomain = Convert.ToString(dt.Rows[i]["CompanyDomain"]);
                        _resp.AccountContactNumber = Convert.ToString(dt.Rows[i]["AccountContact"]);
                        _resp.AccountEmail = Convert.ToString(dt.Rows[i]["AccountEmail"]);
                        _resp.SupportNumber = Convert.ToString(dt.Rows[i]["SupportNumber"]);
                        _resp.SupportEmail = Convert.ToString(dt.Rows[i]["SupportEmail"]);
                        _resp.OutletName = Convert.ToString(dt.Rows[i]["OutletName"]);
                        _resp.UserName = Convert.ToString(dt.Rows[i]["UserName"]);
                        _resp.UserMobileNo = Convert.ToString(dt.Rows[i]["UserMobileNo"]);
                        _resp.BrandName = Convert.ToString(dt.Rows[i]["BrandName"]);
                        _resp.WID = Convert.ToInt32(dt.Rows[i]["WID"]);
                        _resp.LoginID = Convert.ToInt32(dt.Rows[i]["LoginID"]);
                        List<UserDetail> userDatas = new List<UserDetail>();
                        List<string> TransactionID = new List<string>();
                        if (dt.Rows[i]["Users"].ToString().Contains("|"))
                        {
                            string[] UserDataArr = dt.Rows[i]["Users"].ToString().Split('|');
                            for (int udi = 0; udi < UserDataArr.Length; udi++)
                            {
                                var userData = new UserDetail
                                {
                                    OutletName = UserDataArr[udi].Split((char)160)[0],
                                    MobileNo = UserDataArr[udi].Split((char)160)[1],
                                    EmailID = UserDataArr[udi].Split((char)160)[2],
                                };
                                userDatas.Add(userData);
                                TransactionID.Add(UserDataArr[udi].Split((char)160)[3]);
                            }
                        }
                        else if (dt.Rows[i]["Users"].ToString().Contains((char)160))
                        {
                            var userData = new UserDetail
                            {
                                OutletName = dt.Rows[i]["Users"].ToString().Split((char)160)[0],
                                MobileNo = dt.Rows[i]["Users"].ToString().Split((char)160)[1],
                                EmailID = dt.Rows[i]["Users"].ToString().Split((char)160)[2]
                            };
                            userDatas.Add(userData);
                            TransactionID.Add(dt.Rows[i]["Users"].ToString().Split((char)160)[3]);
                        }
                        _resp.UserData = userDatas;
                        _resp.TransactionID = TransactionID;
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
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateDownStatus";
    }
}
