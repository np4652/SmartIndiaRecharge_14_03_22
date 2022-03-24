using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPriorityApiSwitching : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPriorityApiSwitching(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OpTypeID", req.CommonInt),
                new SqlParameter("@LoginID", req.LoginID)
            };
            var priorityApiSwitchs = new List<PriorityApiSwitch>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var priorityApiSwitch = new PriorityApiSwitch
                    {
                        OID = Convert.ToInt32(dt.Rows[i]["OID"]),
                        Operator = dt.Rows[i]["Operator"].ToString(),
                        OpTypeID = dt.Rows[i]["OpTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["OpTypeID"]),
                        OpType = dt.Rows[i]["OpType"].ToString(),
                        IsActive = Convert.ToBoolean(dt.Rows[i]["IsActive"]),
                        BackupAPIID = Convert.ToInt32(dt.Rows[i]["BackupAPIID"] is DBNull ? 0 : dt.Rows[i]["BackupAPIID"]),
                        BackupAPIIDRetailor = Convert.ToInt32(dt.Rows[i]["BackupAPIIDRetailor"] is DBNull ? 0 : dt.Rows[i]["BackupAPIIDRetailor"]),
                        RealAPIID = Convert.ToInt32(dt.Rows[i]["RealAPIID"] is DBNull ? 0 : dt.Rows[i]["RealAPIID"])
                    };
                    List<APIDetail> ANameIDs = new List<APIDetail>();
                    if (dt.Rows[i]["APINameID"] is DBNull == false)
                    {

                        if (dt.Rows[i]["APINameID"].ToString().Contains((char)160))
                        {
                            string[] APINameIDsArr = dt.Rows[i]["APINameID"].ToString().Split((char)160);
                            for (int ia = 0; ia < APINameIDsArr.Length; ia++)
                            {
                                if (APINameIDsArr[ia].Contains("_"))
                                {
                                    APIDetail ANameID = new APIDetail
                                    {
                                        ID = Convert.ToInt32(APINameIDsArr[ia].Split('_')[0]),
                                        Name = APINameIDsArr[ia].Split('_')[1]
                                    };
                                    ANameIDs.Add(ANameID);
                                }
                            }
                        }
                        else if (dt.Rows[i]["APINameID"].ToString().Contains("_"))
                        {
                            APIDetail ANameID = new APIDetail
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["APINameID"].ToString().Split('_')[0]),
                                Name = dt.Rows[i]["APINameID"].ToString().Split('_')[1]
                            };
                            ANameIDs.Add(ANameID);
                        }
                    }
                    priorityApiSwitch.APINameIDs = ANameIDs;
                    List<APISwitched> switcheds = new List<APISwitched>();
                    if (dt.Rows[i]["APISwitched"] is DBNull == false)
                    {
                        if (dt.Rows[i]["APISwitched"].ToString().Contains((char)160))
                        {
                            string[] APISwitchedArr = dt.Rows[i]["APISwitched"].ToString().Split((char)160);
                            for (int si = 0; si < APISwitchedArr.Length; si++)
                            {
                                if (APISwitchedArr[si].Contains("_"))
                                {
                                    switcheds.Add(new APISwitched
                                    {
                                        APIID = Convert.ToInt32(APISwitchedArr[si].Split('_')[0]),
                                        MaxCount = Convert.ToInt32(APISwitchedArr[si].Split('_')[1]),
                                        IsActive = APISwitchedArr[si].Split('_')[2] == "1" ? true : false,
                                        FailoverCount = Convert.ToInt32(APISwitchedArr[si].Split('_')[3])
                                    });
                                }
                            }
                        }
                        else if (dt.Rows[i]["APISwitched"].ToString().Contains("_"))
                        {
                            switcheds.Add(new APISwitched
                            {
                                APIID = Convert.ToInt32(dt.Rows[i]["APISwitched"].ToString().Split('_')[0]),
                                MaxCount = Convert.ToInt32(dt.Rows[i]["APISwitched"].ToString().Split('_')[1]),
                                IsActive = dt.Rows[i]["APISwitched"].ToString().Split('_')[2] == "1" ? true : false,
                                FailoverCount = Convert.ToInt32(dt.Rows[i]["APISwitched"].ToString().Split('_')[3])
                            });
                        }
                    }
                    priorityApiSwitch.APISwitcheds = switcheds;
                    List<APIDetail> BackupAPIs = new List<APIDetail>();
                    if (dt.Rows[i]["BackupAPIs"] is DBNull == false)
                    {
                        if (dt.Rows[i]["BackupAPIs"].ToString().Contains((char)160))
                        {
                            string[] BackupAPIsArr = dt.Rows[i]["BackupAPIs"].ToString().Split((char)160);
                            for (int ia = 0; ia < BackupAPIsArr.Length; ia++)
                            {
                                if (BackupAPIsArr[ia].Contains("_"))
                                {
                                    BackupAPIs.Add(new APIDetail
                                    {
                                        ID = Convert.ToInt32(BackupAPIsArr[ia].Split('_')[0]),
                                        Name = BackupAPIsArr[ia].Split('_')[1]
                                    });
                                }
                            }
                        }
                        else if (dt.Rows[i]["BackupAPIs"].ToString().Contains("_"))
                        {
                            BackupAPIs.Add(new APIDetail
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["BackupAPIs"].ToString().Split('_')[0]),
                                Name = dt.Rows[i]["BackupAPIs"].ToString().Split('_')[1]
                            });
                        }
                    }
                    priorityApiSwitch.BackupAPIs = BackupAPIs;
                    priorityApiSwitchs.Add(priorityApiSwitch);
                }

            }
            catch (Exception ex)
            {
                string e = "";
                e = ex.Message;

            }
            return priorityApiSwitchs;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetPriorityApiSwitching";
    }
}
