using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.DB;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using System.Text;
using Newtonsoft.Json;
using System.Drawing;
using System.Net.Http.Headers;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.StaticModel;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using System.Web;
using RoundpayFinTech.AppCode.HelperClass;

namespace RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI
{
    public class WhatsappAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly WhatsappSetting appSetting;
        private readonly IDAL _dal;
        private readonly string MediaUrl = "https://sv.wa.team/v1/media.php?id=";
        public WhatsappAPIML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting();
            //appSetting = WhatsAppSetting(aicode);
        }

        private WhatsappSetting AppSetting()
        {
            var setting = new WhatsappSetting();
            try
            {
                setting = new WhatsappSetting
                {
                    BaseUrl = Configuration["WHATSAPP:BaseUrl"],
                    BearerToken = Configuration["WHATSAPP:BearerToken"],
                    Url = Configuration["WATEAM:URL"],
                    APIKEY = Configuration["WATEAM:API-KEY"],
                    WACID = Configuration["WATEAM:WA-CID"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "WhatsappSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public async Task<SendSessionMessageResponse> SendSessionMessage(string messageText, string WhatsappNumber)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            var reserr = new SendSessionMessageResponse
            {
                statuscode = -1,
                result = "Failed",
                info = "Failed"
            };

            try
            {
                string _URL = appSetting.BaseUrl + "sendSessionMessage/" + WhatsappNumber + "?" + "messageText=" + messageText;
                var header = new Dictionary<string, string>
                {
                    { "Authorization","Bearer "+appSetting.BearerToken },
                    { "Content-Type","multipart/form-data"},
                };
                StringBuilder respex = new StringBuilder("");
                var resp = "";
                try
                {
                    resp = await AppWebRequest.O.CallHWRQueryString_PostAsync(_URL, header).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    respex = new StringBuilder(ex.Message);
                }
                WhatsappReqRes wrr = new WhatsappReqRes();
                wrr.Request = _URL;
                wrr.Response = resp == "" ? respex.ToString() : resp;
                wrr.Method = "SendSessionMessage";
                wrr.ClassName = "WhatsappAPIML";
                IProcedure proc = new ProcSaveWhatsappReqResp(_dal);
               var res = (WhatsappReqRes)proc.Call(wrr);

                if (!string.IsNullOrEmpty(resp))
                {
                    var pattern = "{\"result\":false";
                    var SendSessionMessageResponse = new SendSessionMessageResponse();
                    var failres = new SendSessionMessageResponsefailed();
                    if (resp.Contains(pattern))
                    {
                        failres = JsonConvert.DeserializeObject<SendSessionMessageResponsefailed>(resp);
                        SendSessionMessageResponse.result = failres.result;
                        if (failres.message != null)
                        {
                            SendSessionMessageResponse.info = failres.message;
                        }
                        else
                        {
                            SendSessionMessageResponse.info = failres.info;
                        }
                        SendSessionMessageResponse.ticketStatus = failres.ticketStatus;
                        return SendSessionMessageResponse;
                    }
                    else
                    {
                        SendSessionMessageResponse = JsonConvert.DeserializeObject<SendSessionMessageResponse>(resp);
                        return SendSessionMessageResponse;
                    } // SendSessionMessageResponse.message = "";

                }
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return reserr;
        }
        #region WATEam
        public async Task<SendSessionMessageResponse> WATEAM_SendSessionMessage(WhatsappAPIWaTeam _ObjWteam)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            var reserr = new SendSessionMessageResponse
            {
                statuscode = -1,
                result = "failed",
                info = "failed"
            };
            try
            {
                string _URL = appSetting.Url;
                var header = new Dictionary<string, string>
                {
                    { "API-KEY",appSetting.APIKEY},
                    { "WA-CID",appSetting.WACID}
                };
                StringBuilder respex = new StringBuilder("");
                var resp = "";
                resp = await AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, _ObjWteam, header).ConfigureAwait(false);
                var wrr = new WhatsappReqRes()
                {
                    Request = _URL,
                    Response = resp == "" ? respex.ToString() : resp,
                    Method = "SendSessionMessage",
                    ClassName = "WhatsappAPIML"
                };
                IProcedure proc = new ProcSaveWhatsappReqResp(_dal);
                var saveresponse = (WhatsappReqRes)proc.Call(wrr);
                if (!string.IsNullOrEmpty(resp))
                {
                    var failres = JsonConvert.DeserializeObject<WhatsappAPIWaTeam>(resp);
                    if (failres.success)
                    {
                        reserr.statuscode = 1;
                        reserr.result = "success";
                        reserr.info = "success";
                        foreach (var item in failres.messages)
                        {
                            reserr.conversationId = item.id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return reserr;
        }
        #endregion
        #region AlertHub
        public async Task<SendSessionMessageResponse> AlertHub_SendSessionMessage(WhatsappAPIAlertHub _ObjAlertHub)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            var reserr = new SendSessionMessageResponse
            {
                statuscode = -1,
                result = "failed",
                info = "failed"
            };
            try
            {
                StringBuilder sbDetailUrl = new StringBuilder(_ObjAlertHub.APIURL);
                Random rn = new Random();
                var requestiddetail = DateTime.Now.ToString("yyyymmddMMss") + rn.Next(0000, 9999);
                sbDetailUrl.Replace("{RequestID}", requestiddetail);
                sbDetailUrl.Replace("{COUNTRY}{TO}", _ObjAlertHub.jid);
                sbDetailUrl.Replace("{MESSAGE}", _ObjAlertHub.content);
                sbDetailUrl.Replace("{MESSAGETYPE}", _ObjAlertHub.messagetype);
                sbDetailUrl.Replace("{SCANNO}", _ObjAlertHub.ScanNo);
                sbDetailUrl.Replace("{QUOTEID}", _ObjAlertHub.ConversationID);
                sbDetailUrl.Replace("{QUOTEMSG}", _ObjAlertHub.QuoteMsg);
                sbDetailUrl.Replace("{REPLYJID}", _ObjAlertHub.ReplyJID);
                StringBuilder respex = new StringBuilder("");
                var resp = await AppWebRequest.O.CallUsingWebClient_GETAsync(sbDetailUrl.ToString()).ConfigureAwait(false);
                var wrr = new WhatsappReqRes()
                {
                    Request = sbDetailUrl.ToString(),
                    Response = resp == "" ? respex.ToString() : resp,
                    Method = "SendSessionMessage",
                    ClassName = "WhatsappAPIML"
                };
                IProcedure proc = new ProcSaveWhatsappReqResp(_dal);
                var saveresponse = (WhatsappReqRes)proc.Call(wrr);
                if (!string.IsNullOrEmpty(resp))
                {
                    var failres = JsonConvert.DeserializeObject<WhatsappAPIReqRes>(resp);
                    if (failres.status == "SUCCESS" || failres.status == "PENDING")
                    {
                        reserr.statuscode = 1;
                        reserr.result = "success";
                        reserr.info = "success";
                        reserr.conversationId = failres.conversationId;
                    }
                }
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return reserr;
        }
       
        #endregion
        public async Task<string> GetToken()
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            try
            {
                string token = "Bearer ";
                token = token + appSetting.BearerToken;
                return token;
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return null;
        }


        //public async Task<SendSessionMessageResponse> sendSessionFileMessage(string messageText, string WhatsappNumber, IFormFile  fd)
        //{
        //    var _Request = string.Empty;
        //    var _Response = string.Empty;
        //    try
        //    {
        //        string _URL = string.Empty;

        //            _URL = appSetting.BaseUrl + "sendSessionFile/" + WhatsappNumber + "?" + "caption=" + messageText;
        //        var fileName = ContentDispositionHeaderValue.Parse(fd.ContentDisposition).FileName.Trim('"').Replace(" ", "");
        //       var extension = Path.GetExtension(fd.FileName);
        //        extension = extension.Substring(1);
        //        var mimetype ="";
        //        if (extension == "gif" || extension == "png" || extension == "jpeg" || extension == "jpg")
        //        {
        //            mimetype = "image/" + "png";
        //        }
        //        else if (extension == "mp4" || extension == "m4v")
        //        {
        //            mimetype = "video/" + extension;
        //        }
        //        else
        //        {
        //            mimetype = "form-data";
        //        }
        //        var header = new Dictionary<string, string>
        //        {
        //            { "Authorization","Bearer "+appSetting.BearerToken },
        //            { "Content-Type","multipart/form-data"},
        //        };
        //            StringBuilder sb = new StringBuilder();
        //            sb.Append(DOCType.WatsappImagePath);
        //            if (!Directory.Exists(sb.ToString()))
        //            {
        //                Directory.CreateDirectory(sb.ToString());
        //            }
        //          //  string filename = WhatsappNumber.ToString() + DateTime.Now.ToString("yyyymmddMMss");
        //            sb.Append(fileName);
        //           // sb.Append(extension);

        //            using (FileStream fs = File.Create(sb.ToString()))
        //            {
        //                fd.CopyTo(fs);
        //                fs.Flush();
        //            }

        //        var resp = await AppWebRequest.O.UploadFilesToRemoteUrl(fileName,mimetype, _URL, fd, header).ConfigureAwait(false);
        //        if (!string.IsNullOrEmpty(resp))
        //        {
        //            var pattern = "{\"result\":false";
        //            var SendSessionMessageResponse = new SendSessionMessageResponse();
        //            var failres = new SendSessionMessageResponsefailed();
        //            if (resp.Contains(pattern))
        //            {
        //                failres = JsonConvert.DeserializeObject<SendSessionMessageResponsefailed>(resp);
        //                SendSessionMessageResponse.result = failres.result;
        //                if (failres.message != null)
        //                {
        //                    SendSessionMessageResponse.info = failres.message;
        //                }
        //                else
        //                {
        //                    SendSessionMessageResponse.info = failres.info;
        //                }
        //                SendSessionMessageResponse.ticketStatus = failres.ticketStatus;
        //                return SendSessionMessageResponse;
        //            }
        //            else
        //            {
        //                SendSessionMessageResponse = JsonConvert.DeserializeObject<SendSessionMessageResponse>(resp);
        //                return SendSessionMessageResponse;
        //            } // SendSessionMessageResponse.message = "";

        //        }
        //        FileInfo file = new FileInfo("Image/WatsappImage/" + fileName);
        //        if (file.Exists)//check file exsit or not  
        //        {
        //            file.Delete();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _Response = "Exception:" + ex.Message + "|" + _Response;
        //        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "SendSessionMessage",
        //            Error = ex.Message,
        //            LoginTypeID = LoginType.ApplicationUser
        //        });
        //    }
        //    return null;
        //}
        public async Task<SendSessionMessageResponse> sendSessionFileMessage(string messageText, string WhatsappNumber, IFormFile fd, string mimetype, string filename)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            var reserr = new SendSessionMessageResponse
            {
                statuscode = -1,
                result = "Failed",
                info = "Failed"
            };
            try
            {
                string _URL = string.Empty;

                messageText = String.IsNullOrEmpty(messageText) ? "." : messageText;
                _URL = appSetting.BaseUrl + "sendSessionFile/" + WhatsappNumber + "?" + "caption=" + messageText;
                if (mimetype == "image")
                {
                    mimetype = "image/" + "png";
                }
                else if (mimetype == "video")
                {
                    mimetype = "video/" + "mv4";
                }
                else
                {
                    mimetype = "form-data";
                }
                var header = new Dictionary<string, string>
                {
                    { "Authorization","Bearer "+appSetting.BearerToken },
                    { "Content-Type","multipart/form-data"},
                };
                StringBuilder respex = new StringBuilder("");
                var resp = "";
                try
                {
                    resp = await AppWebRequest.O.UploadFilesToRemoteUrl(filename, mimetype, _URL, fd, header).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    respex = new StringBuilder(ex.Message);
                }
                var wrr = new WhatsappReqRes()
                {
                    Request = _URL,
                    Response = resp == "" ? respex.ToString() : resp,
                    Method = "sendSessionFileMessage",
                    ClassName = "WhatsappAPIML"
                };
                // var saveresponse = whatsappML.ProcWhatsappReqRes(wrr);
                IProcedure proc = new ProcSaveWhatsappReqResp(_dal);
                var saveresponse = (WhatsappReqRes)proc.Call(wrr);
                if (!string.IsNullOrEmpty(resp))
                {
                    var pattern = "{\"result\":false";
                    var SendSessionMessageResponse = new SendSessionMessageResponse();
                    var failres = new SendSessionMessageResponsefailed();
                    if (resp.Contains(pattern))
                    {
                        failres = JsonConvert.DeserializeObject<SendSessionMessageResponsefailed>(resp);
                        SendSessionMessageResponse.result = failres.result;
                        if (failres.message != null)
                        {
                            SendSessionMessageResponse.info = failres.message;
                        }
                        else
                        {
                            SendSessionMessageResponse.info = failres.info;
                        }
                        SendSessionMessageResponse.ticketStatus = failres.ticketStatus;
                        return SendSessionMessageResponse;
                    }
                    else
                    {
                        SendSessionMessageResponse = JsonConvert.DeserializeObject<SendSessionMessageResponse>(resp);
                        FileInfo file = new FileInfo("Image/WatsappImage/" + filename);
                        if (file.Exists)//check file exsit or not  
                        {
                            file.Delete();
                        }

                        SendSessionMessageResponse.message.text = appSetting.BaseUrl + "getMedia?fileName=" + SendSessionMessageResponse.message.text;
                        return SendSessionMessageResponse;
                    } // SendSessionMessageResponse.message = "";

                }
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }


            return reserr;
        }

        public async Task<byte[]> GetWatsappFile(string url)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            try
            {
                var header = new Dictionary<string, string>
                {
                    { "Authorization","Bearer "+appSetting.BearerToken },
                    //{ "Content-Type","application/json"},
                };
                return await AppWebRequest.O.CallUsingHttpWebRequest_GETImageAsync(url, header).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return null;
        }
        public async Task<byte[]> GetWatsappFileWATEAM(string url)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            try
            {
                url = MediaUrl + url;
                var header = new Dictionary<string, string>
                {
                    { "API-KEY",appSetting.APIKEY},
                    { "WA-CID",appSetting.WACID}
                };
                return await AppWebRequest.O.CallUsingHttpWebRequest_GETImageAsync(url, header).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return null;
        }
        public async Task<byte[]> GetWatsappVideoFile(string url)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            try
            {
                var header = new Dictionary<string, string>
                {
                    { "Authorization","Bearer "+appSetting.BearerToken },
                    { "Content-Type","application/json"},
                };
                var response = "";
                return await AppWebRequest.O.CallUsingHttpWebRequest_GETVideoAsync(url, header).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return null;
        }


        public async Task<WhatsappGetContactResp> GetContactList()
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            try
            {
                string _URL = appSetting.BaseUrl + "getContacts";
                var header = new Dictionary<string, string>
                {
                    { "Authorization","Bearer "+appSetting.BearerToken },
                    { "Content-Type","multipart/form-data"},
                };
                var resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL, header).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(resp))
                {
                    WhatsappGetContactResp WhatsappGetContactResp = JsonConvert.DeserializeObject<WhatsappGetContactResp>(resp);
                    return WhatsappGetContactResp;
                }

            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetWhatsappContact",
                    Error = ex.Message,
                    LoginTypeID = LoginType.CustomerCare
                });
            }
            return null;

        }

        public async Task<WhatsappGetMessageTempResp> WhatsappGetMessageTemplate()
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            try
            {
                string _URL = appSetting.BaseUrl + "getMessageTemplates";
                var header = new Dictionary<string, string>
                {
                    { "Authorization","Bearer "+appSetting.BearerToken },
                    { "Content-Type","multipart/form-data"},
                };
                var resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL, header).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(resp))
                {
                    WhatsappGetMessageTempResp WhatsappGetMessageTempResp = JsonConvert.DeserializeObject<WhatsappGetMessageTempResp>(resp);
                    return WhatsappGetMessageTempResp;
                }

            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetWhatsappContact",
                    Error = ex.Message,
                    LoginTypeID = LoginType.CustomerCare
                });
            }
            return null;

        }
     
        public async Task<AlertHubWhatsapGroup> GetWhatsappGroups(string GroupListURl,string GroupDetailURL, int apiid,string SenderNo)
        {
            var _Request = string.Empty;
            var _Response = string.Empty;
            var _retres = new AlertHubWhatsapGroup
            {
                StautusCode = -1,
                Msg = "Failed"
            };
            try
            {
                StringBuilder sbListUrl = new StringBuilder(GroupListURl);
             
                //Group List
                Random rn = new Random();
                var requestid = DateTime.Now.ToString("yyyymmddMMss") + rn.Next(0000, 9999);
                sbListUrl.Replace("{ScanNo}", SenderNo);
                sbListUrl.Replace("{RequestID}", requestid);
                var resp = await AppWebRequest.O.CallUsingWebClient_GETAsync(sbListUrl.ToString()).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(resp))
                {
                    var res = JsonConvert.DeserializeObject<AlertHubWhatsapGroup>(resp);
                    if (res.status == "SUCCESS")
                    {
                        SaveWhtsappGroup clsobjswg = new SaveWhtsappGroup();
                        clsobjswg.SenderNo = SenderNo;
                        clsobjswg.Apiid = apiid;
                        //Group Details
                      
                        foreach (var item in res.data.groups)
                        {
                            StringBuilder sbDetailUrl = new StringBuilder(GroupDetailURL);
                            var requestiddetail = DateTime.Now.ToString("yyyymmddMMss") + rn.Next(0000, 9999);
                            clsobjswg.GroupID = item;
                           
                            sbDetailUrl.Replace("{ScanNo}", SenderNo);
                            sbDetailUrl.Replace("{RequestID}", requestiddetail);
                            sbDetailUrl.Replace("{GroupID}", item);
                            var respgroupdetail = await AppWebRequest.O.CallUsingWebClient_GETAsync(sbDetailUrl.ToString()).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(respgroupdetail))
                            {
                                var resdetails = JsonConvert.DeserializeObject<AlertHubWhatsapGroup>(respgroupdetail);
                                if (resdetails.status == "SUCCESS")
                                {
                                    clsobjswg.GroupName = resdetails.data.group_subject;
                                    foreach (var items in resdetails.data.group_participants)
                                    {
                                        if (items.isAdmin == 1)
                                        {
                                            clsobjswg.MobileNo = items.jid;
                                            break;
                                        }
                                    }
                                }
                            }
                            IProcedure proc = new ProcSaveWhatsAppGroup(_dal);
                            var succres = (WhatsappConversation)proc.Call(clsobjswg);
                            if (succres.Statuscode != 1)
                            {
                                return _retres;
                            }
                        }
                    }

                    _retres.StautusCode = 1;
                    _retres.Msg = "Success";
                }

            }
            catch (Exception ex)
            {
                _Response = "Exception:" + ex.Message + "|" + _Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return _retres;
        }
    }
}
