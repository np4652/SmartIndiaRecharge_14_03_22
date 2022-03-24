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

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetVendorMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVendorMaster(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MasterVendorModel)obj;

            var res = new List<MasterVendorModel>();
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LoginID", _req.LoginID),
                    new SqlParameter("@LT", _req.LoginTypeID),
                    new SqlParameter("@Id", _req.ID)
                };
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i <= dt.Rows.Count; i++)
                    {
                        var resItem = new MasterVendorModel
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            VendorName = dt.Rows[i]["_Name"].ToString(),
                            Remark = dt.Rows[i]["_Remark"] is DBNull ? string.Empty : dt.Rows[i]["_Remark"].ToString(),
                            IsActive = dt.Rows[i]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsActive"])
                        };
                        res.Add(resItem);
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetVendorMaster";
    }

    public class ProcVendorMasterCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVendorMasterCU(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MasterVendorModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@LoginID", _req.LoginID),
                    new SqlParameter("@LT", _req.LoginTypeID),
                    new SqlParameter("@Id", _req.ID),
                    new SqlParameter("@Vendor", _req.VendorName),
                    new SqlParameter("@Remark", _req.Remark)
                };
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_VendorMasterCU";
    }

    public class ProcVendorMasterToggle : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVendorMasterToggle(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MasterVendorModel)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                string query = "update tbl_API set _ActiveSts = " + (_req.IsActive == true ? "1" : "0") + " where _id = " + _req.ID.ToString() + "; select 1 as StatusCode, 'Success' as Msg";
                DataTable dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "";
    }

    public class ProcGetVendorOperators : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVendorOperators(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int VendorId = (int)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@VendorId", VendorId);
            VendorBindOperators res = new VendorBindOperators();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    res.ID = Convert.ToInt32(dt.Rows[0]["_Id"]);
                    res.VendorName = dt.Rows[0]["_Name"].ToString();
                    if (dt.Rows[0]["SelectedOperators"].ToString().Trim() != "")
                    {
                        List<int> lst = new List<int>();
                        if(dt.Rows[0]["SelectedOperators"].ToString().Contains((char)160))
                        {
                            string[] CWArr = dt.Rows[0]["SelectedOperators"].ToString().Split((char)160);
                            for (int cwa = 0; cwa < CWArr.Length; cwa++)
                            {
                                lst.Add(Convert.ToInt32(CWArr[cwa]));
                            }
                        }
                        else
                        {
                            lst.Add(Convert.ToInt32(dt.Rows[0]["SelectedOperators"]));
                        }
                        res.SelectedOperators = lst;
                    }
                    //if (dt.Rows[0]["OperatorList"].ToString().Contains((char)160))
                    //{
                    //    List<OpTypeMaster> lst = new List<OpTypeMaster>();
                    //    string[] CWArr = dt.Rows[0]["OperatorList"].ToString().Split((char)160);
                    //    for (int cwa = 0; cwa < CWArr.Length; cwa++)
                    //    {
                    //        if (CWArr[cwa].Contains("_"))
                    //        {
                    //            OpTypeMaster op = new OpTypeMaster
                    //            {
                    //                ID = Convert.ToInt32(CWArr[cwa].Split('_')[0]),
                    //                OpType = CWArr[cwa].Split('_')[1]
                    //            };
                    //            lst.Add(op);
                    //        }
                    //    }
                    //    res.OperatorDdl = lst;
                    //}
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetVendorOperators";
        }
    }

    public class ProcVendorMasterOperatorCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVendorMasterOperatorCU(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (VendorBindOperators)obj;

            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                SqlParameter[] param = {
                    new SqlParameter("@VendorId", _req.ID),
                    new SqlParameter("@StrOp", _req.SelectOps)
                };
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"].ToString();
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
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_VendorOperatorCU";
    }
}
