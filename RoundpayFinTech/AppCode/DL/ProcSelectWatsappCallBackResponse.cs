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
using Newtonsoft.Json;
using Fintech.AppCode.WebRequest;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSelectWatsappCallBackResponse : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSelectWatsappCallBackResponse(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new WhatsappReceiveMsgResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName());
                DataTable dt = new DataTable();
                //DataTable dtCus = new DataTable();
                dt = ds.Tables[0];
                //dtCus = ds.Tables[1];
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var response = new WhatsappReceiveMsgResp
                        {
                            _id = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            _Content = row["_Content"] is DBNull ? "" : row["_Content"].ToString()
                           
                        };
                        WhatsappReceiveMsgResp jsonresponse = JsonConvert.DeserializeObject<WhatsappReceiveMsgResp>(response._Content);
                        jsonresponse._id = response._id;
                        jsonresponse.LoginTypeID = req.LoginTypeID;
                        var responsedata = new WhatsappReceiveMsgResp();
                        IProcedure proc = new ProcSaveWhatsappReceiceMessage(_dal);
                        responsedata = (WhatsappReceiveMsgResp)proc.Call(jsonresponse);
                        if (responsedata.Statuscode == 1)
                        {
                            res.Statuscode = responsedata.Statuscode;
                            res.Msg = responsedata.Msg;
                            // res1.Add(res);
                        }
                        else
                        {
                            res.Statuscode = responsedata.Statuscode;
                            res.Msg = responsedata.Msg;
                            // res1.Add(res);
                        }

                    }
                }

            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_SelectWatsappCallBackResponse";
    }
}
