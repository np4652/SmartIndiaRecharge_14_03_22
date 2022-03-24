using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.DL;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System.Text.RegularExpressions;
using Fintech.AppCode.WebRequest;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class WhatsappML : IWhatsappML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;

        public WhatsappML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }

        public GetWhatsappContact getWhatsappContactList()
        {
            throw new NotImplementedException();
        }

        public GetWhatsappContactListModel ProcGetWhatsappContacts(CommonReq _req)
        {
            var res = new GetWhatsappContactListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetWhatsappContact(_dal);
                res = (GetWhatsappContactListModel)proc.Call(_req);
            }
            return res;
        }

        public GetWhatsappContactListModel GetWhatsappSenderNoList(CommonReq _req)
        {
            var res = new GetWhatsappContactListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSelectSenderNo(_dal);
                res = (GetWhatsappContactListModel)proc.Call(_req);
            }
            return res;
        }

        public GetWhatsappContactListModel ProcGetWhatsappContactsSearch(CommonReq _req)
        {
            var res = new GetWhatsappContactListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetWhatsappContactsSearch(_dal);
                res = (GetWhatsappContactListModel)proc.Call(_req);
            }
            return res;
        }

        public GetWhatsappContactListModel ProcSearchUserContacts(CommonReq _req)
        {
            var res = new GetWhatsappContactListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSearchUserContacts(_dal);
                res = (GetWhatsappContactListModel)proc.Call(_req);
            }
            return res;
        }


        public GetWhatsappContactListModel GetWhatsappConversation(CommonReq _req)
        {
            var res = new GetWhatsappContactListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetWhatsappConversation(_dal);
                res = (GetWhatsappContactListModel)proc.Call(_req);
                if (res != null)
                {
                    foreach (var item in res.WhatsappConversations)
                    {
                        item.Text = WhatsappBoldOrItalic(item.Text);
                        item.QuoteMsg = WhatsappBoldOrItalic(item.QuoteMsg);
                    }
                }
            }
            return res;
        }

        public string WhatsappBoldOrItalic(string input)
        {
            //for bold
            string patternbold = @"\*([^*]+)\*";
            string substitutionbold = @"<b>$1</b>";
            Regex regex = new Regex(patternbold);
            input = regex.Replace(input, substitutionbold);

            //for italic
            string patternitalic = @"(_)([^_]+?)(\1)";
            string substitutionitalic = @"<i>$2</i>";
            RegexOptions options = RegexOptions.Multiline;
            Regex regexitalic = new Regex(patternitalic, options);
            input = regexitalic.Replace(input, substitutionitalic);

            //% 0a and /n and /r/n to <br> for Line Break
            input = input.Replace("%0a", "<br/>").Replace("/n", "<br/>").Replace("/r/n", "<br/>");
            // returned final output
            return input;
        }
        public string FormatacItalic(string UText)
        {
            string iss = string.Empty;
            if (UText != "")
            {
                int ac = 1;
                int us = 1;
                int flagspaceac = 0;
                int flagspaceus = 0;

                int acrep = 0;
                int usrep = 0;
                for (int i = 0; i < UText.Length; i++)
                {
                    if (UText[i] == ' ')
                    {
                        if (acrep == 2)
                        {
                            iss = iss + (UText[i] + "");
                            iss = iss.Replace("<b> ", "*");
                            ac--;
                            acrep = 0;
                        }
                        if (usrep == 2)
                        {
                            iss = iss + (UText[i] + "");
                            iss = iss.Replace("<i> ", "_");
                            us--;
                            usrep = 0;
                        }
                        flagspaceac = 1;
                        flagspaceus = 1;
                    }
                    if (flagspaceus == 1 && UText[i] != '_')
                    {
                        flagspaceus = 0;
                    }
                    else if (flagspaceus == 1 && UText[i] == '_')
                    {
                        flagspaceus = 0;
                    }
                    if (flagspaceac == 1 && UText[i] != '*')
                    {
                        flagspaceac = 0;
                    }
                    else if (flagspaceac == 1 && UText[i] == '*')
                    {
                        flagspaceac = 0;
                    }
                    if (UText[i] == '*')
                    {
                        if (flagspaceac == 0)
                        {
                            if (ac % 2 == 0)
                            {
                                iss = iss + "</b>";

                                acrep = 1;
                                ac++;
                            }
                            else
                            {
                                iss = iss + "<b>";

                                acrep = 2;
                                ac++;
                            }
                        }
                        else
                        {
                            iss = iss + (UText[i] + "");
                        }

                    }
                    else if (UText[i] == '_')
                    {
                        if (flagspaceus == 0)
                        {
                            if (us % 2 == 0)
                            {
                                iss = iss + "</i>";
                                usrep = 1;
                                us++;
                            }
                            else
                            {
                                iss = iss + "<i>";
                                usrep = 2;
                                us++;
                            }
                        }
                        else
                        {
                            iss = iss + (UText[i] + "");
                        }
                    }
                    else
                    {
                        iss = iss + (UText[i] + "");
                        acrep = 0;
                        usrep = 0;
                    }
                }
            }
            return iss;
        }

        public GetWhatsappContact ProcSaveWhatsappContacts(CommonReq _req)
        {
            var res = new GetWhatsappContact();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSaveWhatsappContacts(_dal);
                res = (GetWhatsappContact)proc.Call(_req);
            }
            return res;
        }

        public WhatsappMessageTemplate ProcSaveWhatsappMsgTemplate(CommonReq _req)
        {
            var res = new WhatsappMessageTemplate();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSaveWhatsappMsgTemplate(_dal);
                res = (WhatsappMessageTemplate)proc.Call(_req);
            }
            return res;
        }


        public WhatsappConversation ProcSaveWhatsappSendMessage(CommonReq _req)
        {
            var res = new WhatsappConversation();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSaveWhatsappSendMessage(_dal);
                res = (WhatsappConversation)proc.Call(_req);
            }
            return res;
        }

        //async Task<SendSessionMessageResponse> WhatsappSendSessionMessage(string Wm, string Wn) 
        //{
        //    var wAPIML = new WhatsappAPIML(_accessor,_env,_dal);
        //    SendSessionMessageResponse sendWhatsappMessage = await wAPIML.SendSessionMessage(Wm, Wn);
        //    return sendWhatsappMessage;
        //}

        async Task<WhatsappGetContactResp> WhatsappGetContact()
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            WhatsappGetContactResp whatsappGetContactResp = await wAPIML.GetContactList();
            return whatsappGetContactResp;
        }

        //async Task<WhatsappMessageTemplate> WhatsappGetMessageTemplate()
        //{
        //    var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
        //    WhatsappMessageTemplate whatsappMessageTemplate = await wAPIML.WhatsappGetMessageTemplate();
        //    return whatsappMessageTemplate;
        //}

        async Task<WhatsappGetContactResp> IWhatsappML.WhatsappGetContact()
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            WhatsappGetContactResp whatsappGetContactResp = await wAPIML.GetContactList();
            return whatsappGetContactResp;
        }

        async Task<WhatsappGetMessageTempResp> IWhatsappML.WhatsappGetMessageTemplate()
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            WhatsappGetMessageTempResp WhatsappGetMessageTempResp = await wAPIML.WhatsappGetMessageTemplate();
            return WhatsappGetMessageTempResp;
        }

        async Task<SendSessionMessageResponse> IWhatsappML.WhatsappSendSessionMessage(WhatsappConversation wc)
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            var res = new SendSessionMessageResponse
            {
                statuscode = -1,
                result = "Failed",
                info = "Failed"
            };

            if (wc.APICODE == WhatsappAPICode.WATI)
            {
                return await wAPIML.SendSessionMessage(wc.Text, wc.ContactId);
            }
            else if (wc.APICODE == WhatsappAPICode.WATEAM)
            {

                var objWATEAM = new WhatsappAPIWaTeam
                {
                    to = wc.ContactId,
                    type = "text",
                    recipient_type = "individual",
                    //texta = new text {body = wc.Text }
                    text = new text() { body = wc.Text }
                };
                return await wAPIML.WATEAM_SendSessionMessage(objWATEAM);
            }
            else
            {
                return res;
            }
            return res;
        }
        async Task<SendSessionMessageResponse> IWhatsappML.WhatsappForwardSessionMessage(string FDMsgs)
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            ForwardMessage fdmsgs = JsonConvert.DeserializeObject<ForwardMessage>(FDMsgs);
            var sendWhatsappMessage = new SendSessionMessageResponse();
            foreach (var item in fdmsgs.Mobile)
            {
                sendWhatsappMessage = await wAPIML.SendSessionMessage(fdmsgs.MessageText, item.MT);
            }
            return sendWhatsappMessage;
        }
        async Task<SendSessionMessageResponse> IWhatsappML.sendSessionFileMessage(string Wm, string Wn, IFormFile fd, string mimetype, string filename)
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            SendSessionMessageResponse sendWhatsappMessage = await wAPIML.sendSessionFileMessage(Wm, Wn, fd, mimetype, filename);
            return sendWhatsappMessage;
        }
        async Task<byte[]> IWhatsappML.GetWatsappFile(string url, string ac)
        {
            byte[] bytes = new byte[1600000];
            if (ac == WhatsappAPICode.WATI)
            {
                var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
                bytes = await wAPIML.GetWatsappFile(url);
            }
            else if (ac == WhatsappAPICode.WATEAM)
            {
                var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
                bytes = await wAPIML.GetWatsappFileWATEAM(url);
            }
            return bytes;

        }
        async Task<byte[]> IWhatsappML.GetWatsappVideoFile(string url)
        {
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            return await wAPIML.GetWatsappVideoFile(url);
        }


        //  base64 to iform file
        private IFormFile base64tofile(string imgestring)
        {
            IFormFile files = null;
            try
            {
                byte[] byteArray = System.Convert.FromBase64String(imgestring.Replace("data:image/png;base64,", ""));
                var stream = new MemoryStream(byteArray);
                var file = new FormFile(stream, 0, byteArray.Length, "image", "image.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png",
                };
                System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = file.FileName
                };
                file.ContentDisposition = cd.ToString();
                return file;
            }
            catch (Exception ex)
            {
                return files;
            }
        }


        public async Task<WhatsappConversation> SendWhatsappSessionMsgByConvID(string TextMessage, string ConversationID)
        {
            var res = new WhatsappConversation
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                if (string.IsNullOrEmpty(TextMessage) || string.IsNullOrEmpty(ConversationID))
                {
                    return res;
                }
                IProcedure proc = new ProcSelectWhatsappMsgByConvID(_dal);
                res = (WhatsappConversation)proc.Call(ConversationID);
                if (res != null)
                {
                    if (res.Statuscode > 0 && !string.IsNullOrEmpty(res.ContactId) && !string.IsNullOrEmpty(res.APICODE) && !string.IsNullOrEmpty(res.SenderNo) && res.CCID > 0 && !string.IsNullOrEmpty(res.CCName))
                    {
                        res.Text = TextMessage;
                        res.LoginTypeID = 1;
                        res.Id = 0;
                        res.Type = "text";
                        var response = SendWhatsappSessionMessageAllAPI(res, true);
                        res.Statuscode = 1;
                        res.Msg = "Success";
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendWhatsappSessionMsgByConvID",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        private async Task uploadFile(WhatsappConversation wc)
        {
            FileStream fs = null;
            wc.Text = string.IsNullOrEmpty(wc.Text) ? "." : wc.Text;
            var fileName = ContentDispositionHeaderValue.Parse(wc.File.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var extension = Path.GetExtension(wc.File.FileName);
            extension = extension.Substring(1);
            var filetype = string.Empty;
            if (extension == "gif" || extension == "png" || extension == "jpeg" || extension == "jpg")
            {
                filetype = "image";
            }
            else if (extension == "mp4" || extension == "m4v")
            {
                filetype = "video";
            }
            else
            {
                filetype = "text";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(DOCType.WatsappImagePath);
            if (!Directory.Exists(sb.ToString()))
            {
                Directory.CreateDirectory(sb.ToString());
            }
            string GenFileName = wc.ContactId.ToString() + DateTime.Now.ToString("yyyymmddMMss") + Path.GetExtension(wc.File.FileName);
            sb.Append(GenFileName);
            // sb.Append(extension);

            using (fs = File.Create(sb.ToString()))
            {
                wc.File.CopyTo(fs);
                fs.Flush();
            }
            wc.Type = filetype;
            wc.Data = "Image/WatsappImage/" + GenFileName;
            if (wc.APICODE == WhatsappAPICode.WATEAM)
            {
                IWebsiteML ml = new WebsiteML(_accessor, _env);
                var websiteSettingModel = new WebsiteSettingModel
                {
                    websiteInfo = ml.GetWebsiteInfo()
                };
                wc.absoluteurl = websiteSettingModel.websiteInfo.AbsoluteHost + "/Image/WatsappImage/" + GenFileName;
                wc.Text = "";
            }
            wc.FileName = GenFileName;
            await Task.Delay(0);
        }

        public async Task<WhatsappConversation> SaveWhatsappConvertation(WhatsappConversation wc)
        {
            IProcedure proc = new ProcSaveWhatsappSendMessage(_dal);
            var res = (WhatsappConversation)proc.Call(wc);
            return res ?? new WhatsappConversation();
        }

        public async Task<WhatsappConversation> SendWhatsappSessionMessageAllAPI(WhatsappConversation wc, bool Isautoreply = false)
        {
            FileStream fs = null;
            SendSessionMessageResponse response = new SendSessionMessageResponse();
            if (!string.IsNullOrEmpty(wc.ContactId))
            {
                wc.ContactId = wc.ContactId.Length == 10 ? "91" + wc.ContactId : wc.ContactId.Length == 12 ? wc.ContactId : wc.ContactId.Length > 12 && wc.ContactId.Contains("91GRP") ? wc.ContactId : "91" + wc.ContactId;
            }

            var res = new WhatsappConversation
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                if (!string.IsNullOrEmpty(wc.conversationId) && string.IsNullOrEmpty(wc.ContactId) || string.IsNullOrEmpty(wc.APICODE))
                {
                    IProcedure procGetData = new ProcSelectWhatsappMsgByConvID(_dal);
                    wc = (WhatsappConversation)procGetData.Call(wc);
                    if (wc != null)
                    {
                        if (wc.Statuscode < 1 || string.IsNullOrEmpty(wc.ContactId) || string.IsNullOrEmpty(wc.APICODE) || string.IsNullOrEmpty(wc.SenderNo) || wc.CCID < 0)
                        {
                            return res;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(wc.Screenshot))
                {
                    wc.File = base64tofile(wc.Screenshot);
                    if (wc.File == null)
                    {
                        return res;
                    }
                }
                if (wc.File != null)
                {
                    await uploadFile(wc);
                }
                wc.StatusString = WhatsappStatusString.Pending;
                res = await SaveWhatsappConvertation(wc);
                //IProcedure proc = new ProcSaveWhatsappSendMessage(_dal);
                //res = (WhatsappConversation)proc.Call(wc);
                res.File = wc.File;
                IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
                wc.StatusString = WhatsappStatusString.UnSent;
                wc.MessageID = res.MessageID;
                if (res.Statuscode == 1)
                {
                    //var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
                    if (wc.APICODE == WhatsappAPICode.WATI)
                    {
                        //Whatsapp Wati Api  Call
                        response = await WhatsappWati(wc, res);
                    }
                    if (wc.APICODE == WhatsappAPICode.WATEAM)
                    {
                        //Whatsapp WATEAM Api  Call
                        response = await WhatsappWateam(wc, res);
                    }
                    if (wc.APICODE == WhatsappAPICode.ALERTHUB)
                    {
                        //Whatsapp ALERTHUB Api Call 
                        response = await WhatsappAlertHub(wc, res);
                    }
                    if (response != null)
                    {
                        if (response.result is "success")
                        {
                            wc.StatusString = WhatsappStatusString.Sent;
                            if (wc.APICODE == WhatsappAPICode.WATI)
                            {
                                wc.conversationId = response.message.conversationId;
                                wc.Data = res.Type != "text" ? response.message.text : "";
                            }
                            else
                            {
                                wc.conversationId = response.conversationId;
                            }
                        }
                        if (wc.MessageID > 0)
                        {
                            whatsappML.ProcUpdatewhatsappMsgStatus(wc);
                            res.Statuscode = 1;
                            res.Msg = "success";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendWhatsappSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = wc.LoginTypeID,
                    UserId = wc.CCID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        private async Task<SendSessionMessageResponse> WhatsappWati(WhatsappConversation wc, WhatsappConversation res)
        {
            //Whatsapp Wati Api Text and MEdia Send Seperately In Api
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            var response = new SendSessionMessageResponse()
            {
                statuscode = ErrorCodes.Minus1,
                msg = ErrorCodes.TempError
            };
            if (wc.Type == "text")
            {
                response = await wAPIML.SendSessionMessage(wc.Text, wc.ContactId);
            }
            else if (wc.Type != "text")
            {
                response = await wAPIML.sendSessionFileMessage(wc.Text, wc.ContactId, wc.File, res.Type, res.FileName).ConfigureAwait(false);
            }
            return response;
        }
        private async Task<SendSessionMessageResponse> WhatsappWateam(WhatsappConversation wc, WhatsappConversation res)
        {
            //Whatsapp Watteam Api Text and MEdia Send  In a Api
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            var objWATEAM = new WhatsappAPIWaTeam
            {
                to = wc.ContactId,
                type = res.Type,
                recipient_type = "individual"
            };
            if (res.Type == "text")
            {
                objWATEAM.text = new text() { body = wc.Text };
            }
            else
            {
                objWATEAM.image = new image() { link = wc.absoluteurl };
            }
            return await wAPIML.WATEAM_SendSessionMessage(objWATEAM);
        }
        private async Task<SendSessionMessageResponse> WhatsappAlertHub(WhatsappConversation wc, WhatsappConversation res)
        {
            //Whatsapp AlertHub Api Text and MEdia Send  In a Api
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            var objAlertHub = new WhatsappAPIAlertHub
            {
                jid = wc.ContactId,
                messagetype = res.Type == "" ? "" : wc.Type.ToUpper(),
                content = wc.Text,
                APIURL = res.APIURL,
                ScanNo = res.SenderNo,
                ConversationID = wc.conversationId,
                QuoteMsg = wc.QuoteMsg,
                ReplyJID = wc.ReplyJID
            };
            return await wAPIML.AlertHub_SendSessionMessage(objAlertHub);
        }

        public dynamic WhatsappSenderNoService(bool IsApi = true)
        {
            List<WhatsappServiceSenderNo> lstres = new List<WhatsappServiceSenderNo>();
            var apires = new WhatsappResponse()
            {
                statuscode = -1,
                msg = "Failed"
            };
            try
            {
                IProcedure _proc = new procSelectWhatsappSenderNoService(_dal);
                lstres = (List<WhatsappServiceSenderNo>)_proc.Call();
                if (IsApi)
                {
                    bool b = ApiWhatsappSenderNoService(lstres);
                    if(b)
                    {
                        apires.statuscode = 1;
                        apires.msg = "Success";
                    }
                    return apires;
                }
            }
            catch (Exception ex)
            {
                if (IsApi)
                    return apires;
                else
                    return lstres;
            }
            return lstres;
        }

        private bool ApiWhatsappSenderNoService(List<WhatsappServiceSenderNo> res)
        {
            try
            {
                if (res != null)
                {
                    foreach (var item in res)
                    {
                        var data = AppWebRequest.O.CallUsingWebClient_GETAsync(item.URL).Result;
                        var wrr = new WhatsappReqRes()
                        {
                            Request = item.URL.ToString(),
                            Response = data.ToString(),
                            Method = "WhatsappSenderNoService",
                            ClassName = "WhatsappML"
                        };
                        IProcedure proc = new ProcSaveWhatsappReqResp(_dal);
                        var saveresponse = (WhatsappReqRes)proc.Call(wrr);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<WhatsappConversation> SendWhatsappForwardMessageAllAPI(string forwardtxt)
        {
            SendSessionMessageResponse response = new SendSessionMessageResponse();
            var res = new WhatsappConversation
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                ForwardMessage fdmsgs = JsonConvert.DeserializeObject<ForwardMessage>(forwardtxt);
                if (!string.IsNullOrEmpty(fdmsgs.MessageText))
                {
                    fdmsgs.MessageText = fdmsgs.MessageText.Replace("<b>", "*").Replace("</b>", "*").Replace("<i>", "_").Replace("</i>", "_").Replace("<br>", "%0a").Replace("&nbsp;", " ");
                }
                foreach (var item in fdmsgs.Mobile)
                {
                    var _req = new WhatsappConversation();
                    {
                        _req.ContactId = item.MT;
                        _req.SenderName = "";
                        _req.Text = fdmsgs.MessageText;
                        _req.LoginTypeID = _lr.LoginTypeID;
                        _req.CCID = _lr.UserID;
                        _req.CCName = _lr.Name;
                        _req.Type = "text";
                        _req.Id = fdmsgs.MsgID;
                        _req.APICODE = fdmsgs.APICODE;
                        _req.SenderNo = fdmsgs.SenderNo;
                    }
                    if (_lr.RoleID > 0)
                    {
                        _req.StatusString = WhatsappStatusString.Pending;
                        IProcedure proc = new ProcSaveWhatsappSendMessage(_dal);
                        res = (WhatsappConversation)proc.Call(_req);

                        IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
                        _req.StatusString = WhatsappStatusString.UnSent;
                        _req.MessageID = res.MessageID;
                        if (res.Statuscode == 1)
                        {
                            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
                            if (_req.APICODE == WhatsappAPICode.WATI)
                            {
                                response = await wAPIML.SendSessionMessage(_req.Text, _req.ContactId);
                            }
                            else if (_req.APICODE == WhatsappAPICode.WATEAM)
                            {
                                var objWATEAM = new WhatsappAPIWaTeam
                                {
                                    to = _req.ContactId,
                                    type = res.Type,
                                    recipient_type = "individual",
                                    text = new text() { body = _req.Text }
                                };
                                response = await wAPIML.WATEAM_SendSessionMessage(objWATEAM);
                            }
                            else if (_req.APICODE == WhatsappAPICode.ALERTHUB)
                            {
                                var objAlertHub = new WhatsappAPIAlertHub
                                {
                                    jid = _req.ContactId,
                                    messagetype = res.Type == "" ? "" : _req.Type.ToUpper(),
                                    content = _req.Text,
                                    APIURL = res.APIURL,
                                    ScanNo = res.SenderNo,
                                    ConversationID = _req.conversationId,
                                    QuoteMsg = _req.QuoteMsg,
                                    ReplyJID = _req.ReplyJID
                                };
                                response = await wAPIML.AlertHub_SendSessionMessage(objAlertHub);
                            }
                            if (response != null)
                            {
                                if (response.result is "success")
                                {
                                    _req.StatusString = WhatsappStatusString.Sent;
                                }
                                if (_req.MessageID > 0)
                                {
                                    whatsappML.ProcUpdatewhatsappMsgStatus(_req);
                                    res.Statuscode = 1;
                                    res.Msg = "success";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendWhatsappSessionMessage",
                    Error = ex.Message,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public WhatsappConversation ProcUpdatewhatsappMsgStatus(WhatsappConversation _req)
        {
            var res = new WhatsappConversation();

            IProcedure proc = new ProcUpdatewhatsappMsgStatus(_dal);
            res = (WhatsappConversation)proc.Call(_req);

            return res;
        }
        public WhatsappReceiveMsgResp WhatsappReceiveMsgResp(CommonReq _req)
        {
            var res = new WhatsappReceiveMsgResp();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSelectWatsappCallBackResponse(_dal);
                res = (WhatsappReceiveMsgResp)proc.Call(_req);
            }
            return res;
        }

        public WhatsappResponse ProcSyncUserWatsappContacts(CommonReq _req)
        {
            var res = new WhatsappResponse();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcSyncUserWatsappContacts(_dal);
                res = (WhatsappResponse)proc.Call(_req);
            }
            return res;
        }

        public WhatsappResponse SelectNewMsg(CommonReq _req)
        {
            var res = new WhatsappResponse();
            IProcedure proc = new ProcSelectNewWhatsappMsg(_dal);
            res = (WhatsappResponse)proc.Call(_req);
            return res;
        }

        public async Task<AlertHubWhatsapGroup> SaveWhatsappGroup(CommonReq req)
        {
            var _retres = new AlertHubWhatsapGroup
            {
                StautusCode = -1,
                Msg = "Failed"
            };
            if (req.CommonInt < 1)
            {
                return _retres;
            }
            if (String.IsNullOrEmpty(req.CommonStr))
            {
                return _retres;
            }
            var wAPIML = new WhatsappAPIML(_accessor, _env, _dal);
            IProcedure proc = new ProcGetWhatsappAPI(_dal);
            var res = (WhatsappAPI)proc.Call(req.CommonInt);
            _retres = await wAPIML.GetWhatsappGroups(res.GroupListUrl, res.GroupDetailUrl, res.APIID, req.CommonStr);
            return _retres;
        }
        public ResponseStatus DeletekeyFromWADictionary(int keyId)
        {
            IProcedure _proc = new ProcDeletekeyFromWADictionary(_dal);
            var res = (ResponseStatus)_proc.Call(keyId);
            return res;
        }

        public ResponseStatus UpdateWhatsappBotDic(WhatsappBotDic req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure _proc = new ProcUpdateWhatsappBotDic(_dal);
            var res = (ResponseStatus)_proc.Call(req);
            return res;
        }
        public IEnumerable<WhatsappBotDic> GetWhatsappBotDicList(CommonReq req)
        {
            IProcedure _proc = new ProcGetWhatsappBotDicList(_dal);
            var res = (List<WhatsappBotDic>)_proc.Call(req);
            return res;
        }

        public ResponseStatus UpdateWhatsappTask(int ContactID, int Task)
        {
            var req = new CommonReq()
            {
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = ContactID,
                CommonInt2 = Task
            };
            IProcedure _proc = new ProcUpdateWhatsappTask(_dal);
            var res = (ResponseStatus)_proc.Call(req);
            return res;
        }

    }
}
