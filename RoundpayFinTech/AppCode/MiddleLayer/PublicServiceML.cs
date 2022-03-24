using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class PublicServiceML : IPublicService
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        /// <summary>
        /// Login response 
        /// it contains login sessions and user related info..
        /// </summary>
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        public PublicServiceML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSessionCheck = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsSessionCheck)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }
        #region Fields operation
        /// <summary>
        /// Gets The Fields List of the related service 
        ///Accepts service id as parameter
        /// </summary>
        public List<FieldMasterModel> GetFieldList(int oid)
        {
            if (oid > 0)
            {
                CommonReq req = new CommonReq
                {
                    CommonInt = 0,
                    CommonInt2 = oid
                };
                IProcedure proc = new ProcGetServiceField(_dal);
                return (List<FieldMasterModel>)proc.Call(req);
            }
            else
            {
                return new List<FieldMasterModel>();
            }
        }
        /// <summary>
        /// Gets dynamic form fields list by id
        /// </summary>
        public FieldMasterModel GetFieldListByID(int id)
        {
            if (id > 0)
            {
                CommonReq req = new CommonReq
                {
                    CommonInt = id,
                    CommonInt2 = 0
                };
                IProcedure proc = new ProcGetServiceField(_dal);
                return (FieldMasterModel)proc.Call(req);
            }
            else
            {
                return new FieldMasterModel();
            }
        }
        /// <summary>
        /// Saves fields for a dynamic form
        /// </summary>
        /// <returns></returns>
        public IResponseStatus SaveField(FieldMasterModel model)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                model._EntryBy = _lr.UserID;
                IProcedure proc = new ProcSaveServiceField(_dal);
                _res = (ResponseStatus)proc.Call(model);
            }
            return _res;
        }

        public FieldMasterModel GetField(int id)
        {
            IProcedure proc = new ProcGetVocab(_dal);
            return (FieldMasterModel)proc.Call();
        }
        #endregion

        public IResponseStatus SaveVocab(VocabMaster model)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (model._Name.Equals(string.Empty) || model._IND.Equals(string.Empty))
            {
                _res.Msg = "Fields can't be empty!";
                return _res;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                model._EntryBy = _lr.UserID;
                IProcedure proc = new ProcSaveVocab(_dal);
                _res = (ResponseStatus)proc.Call(model);
            }
            return _res;
        }
        /// <summary>
        /// Save options for vocab
        /// it shuld be multiple
        /// </summary>
        /// <returns></returns>
        public IResponseStatus SaveVocabOption(VocabList model)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                model._EntryBy = _lr.UserID;
                IProcedure proc = new ProcSaveVocabOption(_dal);
                _res = (ResponseStatus)proc.Call(model);
            }
            return _res;
        }
        /// <summary>
        /// Gets the vocabs master list
        /// </summary>
        /// <returns></returns>
        public List<VocabMaster> GetVocabMaster()
        {
            IProcedure proc = new ProcGetVocab(_dal);
            return (List<VocabMaster>)proc.Call();
        }

        public List<VocabList> GetVocabOption(int vmid)
        {
            IProcedure proc = new ProcGetVocabOptions(_dal);
            return (List<VocabList>)proc.Call(vmid);
        }
        /// <summary>
        /// Draws a dynamic form on the webpage
        /// </summary>
        /// <returns></returns>
        public List<FieldMasterModel> ShowForm(int oid)
        {
            IProcedure proc = new ProcDrawForm(_dal);
            return (List<FieldMasterModel>)proc.Call(oid);
        }
        /// <summary>
        /// Saves all the input values to the database which submitted in dynamic form
        /// </summary>
        /// <param name="formdata"></param>
        /// <returns></returns>
        public IResponseStatus SavePESFormML(IFormCollection formdata)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            SavePESFormModel model = new SavePESFormModel();
            List<FieldValuesModel> lst = new List<FieldValuesModel>();
            var files = formdata.Files;
            var forms = formdata.Keys;
            if (forms.Count > 0)
            {
                foreach (var r in forms)
                {
                    _accessor.HttpContext.Request.Form.TryGetValue(r, out StringValues strVal);
                    if (r.Contains("txt")|| r.Contains("txtarea")|| r.Contains("ddl")||r.Contains("file"))
                    {
                        lst.Add(new FieldValuesModel
                        {
                            FieldID = Convert.ToInt32(r.Replace("txtarea", string.Empty).Replace("area",string.Empty).Replace("file", string.Empty).Replace("txt", string.Empty).Replace("ddl", string.Empty)),
                            FieldValue = strVal.ToString()
                        });
                    }
                    else
                    {
                        if (r=="OID")
                        {
                            model.OID = Convert.ToInt32(strVal);
                        }
                        else if (r== "cname")
                        {
                            model.Customername = strVal.ToString();
                        }
                        else if (r == "cmobno")
                        {
                            model.CustomerMobno = strVal.ToString();
                        }
                    }
                }
            }
            if (files.Count > 0)
            {
                foreach (var f in files)
                {
                    var uploads = Path.Combine(_env.WebRootPath, DOCType.PESFiles);
                    if (f.Length > 0)
                    {
                        string filename = Regex.Replace(_lr.UserID + Path.GetRandomFileName().Replace(" ",string.Empty), @"[^0-9a-zA-Z]+",string.Empty)+ Path.GetExtension(f.FileName);
                        using (var fileStream = new FileStream(Path.Combine(uploads,filename), FileMode.Create))
                        {
                            f.CopyTo(fileStream);
                        }
                        if (f.Name.Contains("file"))
                        {
                            lst.Add(new FieldValuesModel
                            {
                                FieldID = Convert.ToInt32(f.Name.Contains("file") ? f.Name.Replace("file",string.Empty) : f.Name),
                                FieldValue = filename.ToString()
                            });
                        }
                    }
                }
            }

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Retailor_Seller)
            {
                model.UserID = _lr.UserID;
                model.AccountNo = model.CustomerMobno;
                model.APIRequestID = string.Empty;
                model.RequestModeID = RequestMode.PANEL;
                model.RequestIP = _rinfo.GetRemoteIP();
                model.FieldValuesList = lst;
                IProcedure proc = new ProcSavePESForm(_dal);
                _res = (ResponseStatus)proc.Call(model);
            }
            return _res;
        }
    }
}
