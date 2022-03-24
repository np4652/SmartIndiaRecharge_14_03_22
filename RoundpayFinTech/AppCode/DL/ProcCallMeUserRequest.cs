using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCallMeUserRequest : IProcedure
    {
        private readonly IDAL _dal;

        public ProcCallMeUserRequest(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus();
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@UserID", _req.LoginID),
                new SqlParameter("@UserMobile", _req.CommonStr),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            { }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_CallMeUserRequest";

        public List<UserCallMeModel> GetCallMeRequest(int top)
        {
            var callmereqList = new List<UserCallMeModel>();
            SqlParameter[] param = { 
            new SqlParameter("@Top",top)
            };

            var dt = _dal.GetByProcedure("proc_GetCallMeUserRequest", param);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        callmereqList.Add(new UserCallMeModel
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            Name = dt.Rows[i]["_Name"].ToString(),
                            MobileNo = dt.Rows[i]["_MobileNo"].ToString(),
                            StatusID = Convert.ToInt32(dt.Rows[i]["_StatusID"]),
                            Status = dt.Rows[i]["_Status"].ToString(),
                            CallHistory = dt.Rows[i]["_CallHistory"].ToString(),
                            EntryDate = dt.Rows[i]["_EntryDate"].ToString()
                        });
                    }
                }
                catch (Exception)
                {
                }
            }
            return callmereqList;
        }
        public int CallMeRequestCount()
        {
            var dt = _dal.Get("select count(*) as callmecount from tbl_usercallme where _StatusID <> 4");
            return Int32.Parse(dt.Rows[0]["callmecount"].ToString());
        }
    }
}