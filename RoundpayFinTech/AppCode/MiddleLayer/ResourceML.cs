using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class ResourceML : IResourceML, IBannerML
    {
        IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IRequestInfo _rinfo;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        public ResourceML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _rinfo = new RequestInfo(_accessor, _env);
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
        public IResponseStatus UploadProductImage(IFormFile file, LoginResponse _lr, int ProductID, int ProductDetailID, string ImgName, int count)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.ProductImagePath.Replace("{0}", ProductID.ToString()));
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(ProductDetailID.ToString());
                    sb.Append("_");
                    sb.Append(ImgName.Split('#')[1]);
                    sb.Append("_");
                    sb.Append(count.ToString());
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public IEnumerable<BannerImage> GetBanners(string WID)
        {
            var crf = _rinfo.GetCurrentReqInfo();
            var root = Path.Combine(DOCType.BannerUserPath.Replace("{0}", WID.ToString()));
            var res = new List<BannerImage>();
            var _res = Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                FileName = Path.GetFileName(x),
                ResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.BannerUserSuffix.Replace("{0}", WID.ToString()), Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x),
                DbUrl = res

            }).OrderByDescending(x => x.Entrydate).ToList();
            try
            {
                IProcedure proc = new ProcGetBanner(_dal);
                res = (List<BannerImage>)proc.Call();
                for (var i = 0; i < _res.Count; i++)
                {
                    for (var j = 0; j < res.Count; j++)
                    {
                        if (i == j)
                        {
                            _res[i].SiteResourceUrl = res[j].RefUrl;

                        }
                    }
                };
            }
            catch (Exception ex)
            {

            }
            return _res;
        }

        public ResponseStatus UploadProfile(IFormFile file, int UserID, int LoginTypeID)
        {
            var res = Validate.O.IsImageValid(file);
            if (res.Statuscode == ErrorCodes.One)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.ProfileImagePath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(UserID.ToString());
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = LoginTypeID,
                        UserId = UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }

        public IResponseStatus UploadBanners(IFormFile file, string WID, string Url, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (Directory.Exists(DOCType.BannerUserPath.Replace("{0}", WID.ToString())))
            {
                DirectoryInfo d = new DirectoryInfo(DOCType.BannerUserPath.Replace("{0}", WID.ToString()));
                FileInfo[] Files = d.GetFiles("*");
                if (Files.Count() >= 4)
                {
                    _res.Msg = "you are already uploaded 4 banners";
                    return _res;
                }
            }

            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 50)
                {
                    _res.Msg = "File size exceeded! Not more than 50 KB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerUserPath.Replace("{0}", WID.ToString()));
                    //sb.Append("\\");
                    //sb.Append(WID);
                    Directory.CreateDirectory(sb.ToString());
                    sb.Append("\\");
                    var date = DateTime.Now.ToString("yyyyMMddHmsfff");
                    sb.Append(date);
                    sb.Append(ext);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    var dr = new BannerImage
                    {
                        URL = date + ext,
                        RefUrl = Url
                    };
                    IProcedure proc = new ProcAddBAnner(_dal);
                    _res = (ResponseStatus)proc.Call(dr);
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Banner uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        #region Amit
        public IResponseStatus UploadB2CBanners(IFormFile file, string WID, int OpType, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerOpTypePath.Replace("{0}", WID.ToString()).Replace("{1}", OpType.ToString()));
                    //sb.Append(DOCType.BannerUserPath.Replace("{1}", OpType.ToString()));
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append("\\");
                    sb.Append(DateTime.Now.ToString("yyyyMMddHmsfff"));
                    sb.Append(ext);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "B2CBanner uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadB2CBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IEnumerable<BannerImage> GetB2CBanners(string WID, int OpType)
        {
            var root = Path.Combine(DOCType.BannerOpTypePath.Replace("{0}", WID).Replace("{1}", OpType.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            if (Directory.Exists(root))
            {
                return Directory.EnumerateFiles(root).Select(x => new BannerImage
                {
                    FileName = Path.GetFileName(x),
                    ResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.BannerOpTypeSuffix.Replace("{0}", WID).Replace("{1}", OpType.ToString()), Path.GetFileName(x)).ToString(),
                    SiteResourceUrl = new StringBuilder().AppendFormat(DOCType.BannerOpTypeSuffix.Replace("{0}", WID).Replace("{1}", OpType.ToString()), Path.GetFileName(x)).ToString(),
                    Entrydate = File.GetCreationTime(x)
                }).OrderByDescending(x => x.Entrydate);
            }
            else
            {
                return new List<BannerImage>();
            }
        }

        public IResponseStatus RemoveB2CBanners(string FileName, int OpType, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Banner not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerOpTypePath.Replace("{0}", WID.ToString()).Replace("{1}", OpType.ToString()));
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Banner removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        #endregion

        public IResponseStatus SiteUploadBanners(IFormFile file, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerSitePath.Replace("{0}", WID.ToString()));
                    Directory.CreateDirectory(sb.ToString());
                    sb.Append("\\");
                    sb.Append(DateTime.Now.ToString("yyyyMMddHmsfff"));
                    sb.Append(ext);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Banner uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }



        public IResponseStatus RemoveBanners(string FileName, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Banner not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerUserPath.Replace("{0}", WID.ToString()));
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Banner removed successfully!";
                    }
                    IProcedure proc = new ProcGetBanner(_dal);
                    _res = (ResponseStatus)proc.Call(FileName);
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IEnumerable<BannerImage> GetHotelLoaderBanner(string WID)
        {
            var root = Path.Combine(DOCType.HotelLoaderContentPath);
            var crf = _rinfo.GetCurrentReqInfo();
            return Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                SiteFileName = Path.GetFileName(x),
                SiteResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.HotelLoaderContentSufix, Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderByDescending(x => x.Entrydate);
        }
        public IEnumerable<BannerImage> SiteGetBanners(string WID)
        {
            var root = Path.Combine(DOCType.BannerSitePath.Replace("{0}", WID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            return Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                SiteFileName = Path.GetFileName(x),

                SiteResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.BannerSiteSuffix.Replace("{0}", WID.ToString()), Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderByDescending(x => x.Entrydate);
        }
        public IEnumerable<BannerImage> SiteGetServices(int WID, int ThemeID)
        {

            var root = Path.Combine(string.Format(DOCType.BGServicesPath, WID.ToString(), ThemeID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            return Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                SiteFileName = Path.GetFileName(x),
                SiteResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, string.Format(DOCType.BGServicesSuffix, WID.ToString(), ThemeID.ToString()), Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderBy(x => x.Entrydate);
        }

        
            public IResponseStatus RemHotelLoaderBanner(string FileName, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Banner not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.HotelLoaderContentPath);
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Banner removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public IResponseStatus SiteRemoveBanners(string FileName, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Banner not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerSitePath.Replace("{0}", WID.ToString()));
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Banner removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public StringBuilder GetLogoURL(int WID)
        {
            StringBuilder sb = new StringBuilder();
            var crf = _rinfo.GetCurrentReqInfo();
            sb.Append(crf.Scheme);
            sb.Append("://");
            sb.Append(crf.Host);
            if (crf.Port > 0)
            {
                sb.Append(":");
                sb.Append(crf.Port);
            }
            sb.Append("/");
            sb.AppendFormat(DOCType.LogoSuffix, WID);
            return sb;
        }

        #region Ranjana
        public void CreateWebsiteDirectory(int WID, string _FolderType)
        {
            //For Website Folder
            if (_FolderType.Equals(FolderType.Website))
            {
                string _Path = DOCType.WebsiteFolderPath.Replace("{0}", WID.ToString());
                string _PathBanner = DOCType.BannerSitePath.Replace("{0}", WID.ToString());
                string _PathApp = DOCType.BannerUserPath.Replace("{0}", WID.ToString());
                string _PathPopup = DOCType.PopupPath.Replace("{0}", WID.ToString());
                string _PathTheme = DOCType.ThemePath.Replace("{0}", WID.ToString());


                if (!(Directory.Exists(_Path)))
                {
                    Directory.CreateDirectory(_Path);
                    Directory.CreateDirectory(_PathBanner);
                    Directory.CreateDirectory(_PathApp);
                    Directory.CreateDirectory(_PathPopup);
                    Directory.CreateDirectory(_PathTheme);

                }
                string _SFileName = "";
                string _DFileName = "";
                string _TFileName = "";

                string[] _FilePath = Directory.GetFiles(DOCType.DefaultFolderPath.Replace("{0}", "0"));
                foreach (var _Files in _FilePath)
                {
                    _SFileName = Path.GetFileName(_Files);
                    if (_SFileName != "Noimage.png")
                    {
                        _DFileName = _Path + "/" + _SFileName;
                    }
                    else
                    {
                        _DFileName = _PathPopup + "/" + _SFileName;
                    }
                    if (!File.Exists(_DFileName))
                    {
                        File.Copy(_Files, _DFileName);
                    }
                }
                string[] _FilePathTheme = Directory.GetFiles(DOCType.DefaultFolderTheme.Replace("{0}", "1"));
                foreach (var _FilesTheme in _FilePathTheme)
                {
                    _TFileName = Path.GetFileName(_FilesTheme);
                    _DFileName = _PathTheme + "/" + _TFileName;
                    if (!File.Exists(_DFileName))
                    {
                        File.Copy(_FilesTheme, _DFileName);
                    }
                }


            }
        }



        #endregion

        public IResponseStatus UploadPopup(IFormFile file, string WID, LoginResponse _lr, string folderType, string ImageType)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                if (ImageType.Equals(FolderType.AppBeforeLoginPopup) || ImageType.Equals(FolderType.AppAfterLoginPopup))
                {
                    using (var image = Image.FromStream(file.OpenReadStream()))
                    {
                        if (image.Height <= 499 || image.Width <= 299)
                        {
                            _res.Msg = "Minimum Image Heigth 500 and Width 300";
                            return _res;
                        }
                    }
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    if (folderType.Equals(FolderType.Website))
                    {
                        sb.Append(DOCType.PopupPath.Replace("{0}", WID.ToString()));
                        if (ImageType.Equals(FolderType.WebsitePopup))
                            sb.Append("/WebPopup.png");
                        else if (ImageType.Equals(FolderType.BeforeLoginPopup))
                            sb.Append("/BeforeLogin.png");
                        else if (ImageType.Equals(FolderType.AfterLoginPopup))
                            sb.Append("/AfterLogin.png");
                        else if (ImageType.Equals(FolderType.AppBeforeLoginPopup))
                            sb.Append("/AppBeforeLogin.png");
                        else if (ImageType.Equals(FolderType.AppAfterLoginPopup))
                            sb.Append("/AppAfterLogin.png");
                    }
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadPopup",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }




        #region  Popup Section
        public IResponseStatus RemovePopup(string FileName, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Banner not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.PopupPath.Replace("{0}", WID.ToString()));
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "PopUp removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IEnumerable<BannerImage> GetPopup(string WID)
        {
            var root = Path.Combine(DOCType.BannerUserPath.Replace("{0}", WID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            return Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                PopupFileName = Path.GetFileName(x),
                PopupResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.PopupSuffix.Replace("{0}", WID.ToString()), Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderByDescending(x => x.Entrydate);
        }
        public string GetAfterLoginPoup(string WID)
        {
            return DOCType.PopupSuffix.Replace("{0}", WID) + "/AppAfterLogin.png";
        }
        public IEnumerable<BannerImage> GetWebsitePopup(string WID)
        {
            var root = Path.Combine(DOCType.PopupPath.Replace("{0}", WID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            return Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                FileName = Path.GetFileName(x),
                ResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}", crf.Scheme, crf.HostValue, DOCType.PopupSuffix.Replace("{0}", WID.ToString())).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderByDescending(x => x.Entrydate);
        }


        #endregion
        public IResponseStatus UpLoadQR(IFormFile file, string FileName, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Bank Insert SuccesFully  But Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BankQRPath);
                    sb.Append(FileName);
                    sb.Append(ext);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Bank QR uploaded successfully";
                    _res.CommonStr = FileName + ext;
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in QR uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UpLoadQR",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IResponseStatus UploadReceipt(IFormFile file, int BankID, LoginResponse _lr)
        {
            var fName = "Reciept_" + Convert.ToString(BankID) + "_" + _lr.UserID + "_" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss").Replace("-", "").Replace(":", "").Replace(" ", "");
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Bank inserted successfully  but invalid file format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.PaymentReceipt);
                    sb.Append(fName);
                    sb.Append(ext);
                    if (!Directory.Exists(DOCType.PaymentReceipt))
                    {
                        Directory.CreateDirectory(DOCType.PaymentReceipt);
                    }
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Receipt Upload successfully";
                    _res.CommonStr = fName + ext;
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in QR uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UpLoadReceipt",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IResponseStatus UploadPartnerImages(IFormFile file, string WID, LoginResponse _lr, string ImageType)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (!file.ContentType.Any())
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (file.Length < 1)
            {
                _res.Msg = "Empty file not allowed!";
                return _res;
            }
            if (file.Length / 1024 > 1024)
            {
                _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                return _res;
            }
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string ext = Path.GetExtension(filename);
            byte[] filecontent = null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                filecontent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(filecontent, ext))
            {
                _res.Msg = "Invalid File Format!";
                return _res;
            }
            try
            {
                string ImgUrl = "/" + _lr.UserID.ToString() + WID + ".png";
                StringBuilder sb = new StringBuilder();
                sb.Append(DOCType.PartnerFolderPath);
                if (ImageType.Equals(FolderType.Logo))
                {
                    sb.Append(FolderType.Logo);
                    sb.Append(ImgUrl);
                }
                else if (ImageType.Equals(FolderType.BgImage))
                {
                    sb.Append(FolderType.BgImage);
                    sb.Append(ImgUrl);
                }

                using (FileStream fs = File.Create(sb.ToString()))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = "Image uploaded successfully";
            }
            catch (Exception ex)
            {
                _res.Msg = "Error in Image uploading. Try after sometime...";
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UploadLogo",
                    Error = ex.Message,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }


        public IResponseStatus UploadPESDocument(List<IFormFile> files, int WID, LoginResponse _lr, int TID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {

                if (!Directory.Exists(DOCType.PESDocumentPath.Substring(0, DOCType.PESDocumentPath.LastIndexOf("/"))))
                {
                    Directory.CreateDirectory(DOCType.PESDocumentPath.Substring(0, DOCType.PESDocumentPath.LastIndexOf("/")));
                }
                if (!Directory.Exists(DOCType.PESDocumentPath.Replace("{TID}", TID.ToString())))
                {
                    Directory.CreateDirectory(DOCType.PESDocumentPath.Replace("{TID}", TID.ToString()));
                }
                int fileCount = 0;
                foreach (var file in files)
                {
                    fileCount++;
                    if (!file.ContentType.Any())
                    {
                        _res.Msg = "File not found!";
                        return _res;
                    }
                    if (file.Length < 1)
                    {
                        _res.Msg = "Empty file not allowed!";
                        return _res;
                    }
                    if (file.Length / 1024 > 1024)
                    {
                        _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                        return _res;
                    }
                    var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string ext = Path.GetExtension(filename);
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    if (!Validate.O.IsFileAllowed(filecontent, ext))
                    {
                        _res.Msg = "Invalid File Format!";
                        return _res;
                    }
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.PESDocumentPath.Replace("{TID}", TID.ToString()));
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }
                        sb.Append("\\");
                        sb.Append(TID.ToString() + "_" + fileCount);
                        sb.Append(ext);
                        using (FileStream fs = File.Create(sb.ToString()))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Files uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        _res.Msg = "Error in banner uploading. Try after sometime...";
                        ErrorLog errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "UploadPESDocument",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }
                }
            }
            return _res;
        }
        public IResponseStatus UpLoadApk(IFormFile file, string FileName, bool IsTest, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (!file.ContentType.Equals("application/vnd.android.package-archive"))
                {
                    _res.Msg = "File not found!!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                if (!ext.Equals(".apk"))
                {
                    _res.Msg = "File type not allowed!";
                    return _res;
                }
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsValidAPK(filecontent))
                {
                    _res.Msg = "Invalid App";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat(DOCType.ApkPath, IsTest ? "Test/" : "");
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(FileName);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Apk is uploaded";
                    _res.CommonStr = FileName;
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in APK uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UpLoadApk",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public CertificateModel downloadPdf(int LT, int UserID, int WID)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = LT,
                LoginID = UserID
            };
            var result = new CertificateModel();
            string _certDocName = DOCType.WebsiteFolderPath.Replace("{0}", WID.ToString());
            string _signImg = DOCType.CertificateImgSignSuffix.Replace("{0}", WID.ToString());
            string _footerCertImg = DOCType.CertificateFooterImageSuffix.Replace("{0}", WID.ToString());
            string _TapitsImg = DOCType.CertificateTapits.Replace("{0}", WID.ToString());
            string _BackgroundImg = DOCType.CertificateBackGround.Replace("{0}", WID.ToString());
            var s = Directory.GetFiles(_certDocName);
            if (!s.Any(e => s.Contains(_signImg)) || !s.Any(e => s.Contains(_footerCertImg)) || !s.Any(e => s.Contains(_TapitsImg)))
            {
                result.Statuscode = ErrorCodes.Minus1;
                result.Msg = "Details not found! Please contact to Admin.";
                return result;
            }
            IProcedure proc = new ProcPdfData(_dal);
            var res = (ResponseStatus)proc.Call(req);
            IProcedure procC = new ProcCompanyProfile(_dal);
            var companyDetail = (CompanyProfileDetail)procC.Call(WID);
            result.joiningDate = res.CommonStr;
            result.Name = companyDetail.Name;
            result.Address = companyDetail.Address;
            result.website = companyDetail.website;
            result.PhoneNo = companyDetail.PhoneNo;
            result.mobileNo = companyDetail.mobileNo;
            result.EmailId = companyDetail.EmailId;
            result.OwnerName = companyDetail.OwnerName;
            result.OwnerDesignation = companyDetail.OwnerDesignation;
            result.KYCStatus = res.CommonInt;
            result.Pan = res.CommonStr2;
            result.UserAddress = res.CommonStr3;

            return result;
        }

        public IResponseStatus UploadImages(IFormFile file, LoginResponse _lr, string folderType, string ImageType, string ImgName = "")
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (folderType.Equals(FolderType.Website))
                {
                    if (!(ext.ToUpper() == ".PNG"))
                    {
                        _res.Msg = "Invalid File Format!";
                        return _res;
                    }
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    var ml = new WebsiteML(_accessor, _env);
                    var _Winfo = ml.GetWebsiteInfo();
                    //var lml = new LoginML(_accessor, _env);
                    //WebsiteInfo _Winfo = lml.GetWebsiteInfo();
                    StringBuilder sb = new StringBuilder();
                    if (folderType.Equals(FolderType.Website))
                    {
                        sb.Append(DOCType.WebsiteFolderPath);
                        sb.Replace("{0}", _lr.WID.ToString());
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }

                        if (!Directory.Exists(sb.ToString() + "/t" + _Winfo.ThemeId.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString() + "/t" + _Winfo.ThemeId.ToString());
                        }
                        if (ImageType.Equals(FolderType.Logo))
                            sb.Append("logo.png");
                        else if (ImageType.Equals(FolderType.whiteLogo))
                            sb.Append("white-logo.png");
                        else if (ImageType.Equals(FolderType.b2cLogo))
                            sb.Append("B2C-logo.png");
                        else if (ImageType.Equals(FolderType.BgImage))
                        {
                            sb.Append("/t{1}/");
                            sb.Replace("{1}", _Winfo.ThemeId.ToString());
                            sb.Append("bg-main.png");
                        }

                        else if (ImageType.Equals(FolderType.ServiceImage))
                        {
                            sb.Append("/t{1}/");
                            sb.Replace("{1}", _Winfo.ThemeId.ToString());
                            if (string.IsNullOrEmpty(ImgName))
                            {
                                sb.Append("services.png");
                            }
                            else
                            {
                                sb.Append("services/");
                                sb.Append(ImgName);
                                sb.Append(".png");
                            }
                        }
                        else if (ImageType.Equals(FolderType.Sign))
                            sb.Append("Sign.png");
                        else if (ImageType.Equals(FolderType.CertificateFooterImage))
                            sb.Append("CertificateFooterImage.png");
                        else if (ImageType.Equals(FolderType.AppImage))
                            sb.Append("App-logo.png");
                    }
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public IResponseStatus RemoveImage(LoginResponse _lr, string folderType, string ImageType, string ImgName)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                try
                {
                    var ml = new WebsiteML(_accessor, _env);
                    var _Winfo = ml.GetWebsiteInfo();
                    StringBuilder sb = new StringBuilder();
                    if (folderType.Equals(FolderType.Website))
                    {
                        sb.Append(DOCType.WebsiteFolderPath);
                        sb.Replace("{0}", _lr.WID.ToString());
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }

                        if (!Directory.Exists(sb.ToString() + "/t" + _Winfo.ThemeId.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString() + "/t" + _Winfo.ThemeId.ToString());
                        }
                        if (ImageType.Equals(FolderType.Logo))
                            sb.Append("logo");
                        else if (ImageType.Equals(FolderType.whiteLogo))
                            sb.Append("white-logo");
                        else if (ImageType.Equals(FolderType.BgImage))
                        {
                            sb.Append("/t{1}/");
                            sb.Replace("{1}", _Winfo.ThemeId.ToString());
                            sb.Append("bg-main");
                        }

                        else if (ImageType.Equals(FolderType.ServiceImage))
                        {
                            sb.Append("/t{1}/");
                            sb.Replace("{1}", _Winfo.ThemeId.ToString());
                            if (string.IsNullOrEmpty(ImgName))
                            {
                                sb.Append("services");
                            }
                            else
                            {
                                sb.Append("services/");
                                sb.Append(ImgName);
                                //sb.Append(".png");
                            }
                        }
                        else if (ImageType.Equals(FolderType.Sign))
                            sb.Append("Sign.png");
                        else if (ImageType.Equals(FolderType.CertificateFooterImage))
                            sb.Append("CertificateFooterImage.png");
                    }
                    string[] Extensions = { ".png", ".jpeg", ".jpg" };
                    foreach (string s in Extensions)
                    {
                        string ExistFile = sb.ToString() + s;
                        if (File.Exists(ExistFile))
                        {
                            File.Delete(ExistFile);
                        }
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveImages",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    });
                }
            }
            return _res;
        }
      

        public IResponseStatus uploadAfItem(IFormFile file, int VendorID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder FileName = new StringBuilder();
                    FileName.Append("FIle_{VendorID}_{DateTime}.png");
                    FileName.Replace("{VendorID}", VendorID.ToString());
                    FileName.Replace("{DateTime}", DateTime.Now.ToString("dd MM yy hh:mm:ss").Replace(" ", "").Replace(":", ""));
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.AffiliateItemPath.Replace("{0}", VendorID.ToString()));
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(FileName);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                    _res.CommonStr = FileName.ToString();
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public ResponseStatus UploadChannelImage(IFormFile file, LoginResponse _lr, int ChannelID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.ChannelImagePath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(ChannelID.ToString());
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        #region Getvideolink
        public IEnumerable<Video> Getvideolink(LoginResponse _lr)
        {
            var resp = new List<Video>();

            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = 0
            };
            IProcedure _proc = new ProcGetVideoLink(_dal);
            resp = (List<Video>)_proc.Call(req);

            return resp;
        }
        public IEnumerable<Video> GetvideolinkApp(CommonReq req)
        {
            var resp = new List<Video>();
            IProcedure _proc = new ProcGetVideoLink(_dal);
            resp = (List<Video>)_proc.Call(req);

            return resp;
        }
        public IResponseStatus SaveVideo(LoginResponse _lr, int ID, string URL, string Title)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                if (string.IsNullOrEmpty(URL) || Validate.O.IsNumeric(URL ?? ""))
                {

                    resp.Msg = ErrorCodes.InvalidParam + " URL";
                    return resp;
                }
                if (URL.Contains("iframe") == false)
                {
                    resp.Msg = "Invalid URL Format";
                    return resp;
                }
                if (string.IsNullOrEmpty(Title) || Validate.O.IsNumeric(Title ?? ""))
                {
                    resp.Msg = ErrorCodes.InvalidParam + " Title";
                    return resp;
                }
                var _req = new videoDetailReq
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ID = ID,
                    URL = URL,
                    Title = Title
                };
                IProcedure proc = new ProcSaveDeleteVideo(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }
        public IResponseStatus RemoveVideo(LoginResponse _lr, int ID)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {

                var _req = new videoDetailReq
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ID = ID,
                    URL = "",
                    Title = ""

                };
                IProcedure proc = new ProcSaveDeleteVideo(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }
        #endregion
        public ResponseStatus UploadWebNotificationImage(IFormFile file, LoginResponse _lr, string ImageName)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.WebNotificationPath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(ImageName.ToString());
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public ResponseStatus UploadOperatorIcon(IFormFile file, int OID)
        {
            var res = Validate.O.IsImageValid(file);
            if (res.Statuscode == ErrorCodes.One)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.OperatorIconPath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(OID.ToString());
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadOperatorIcon",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }
        public IResponseStatus UploadEmployeeGift(IFormFile file, string filename)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (!file.ContentType.Any())
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (file.Length < 1)
            {
                _res.Msg = "Empty file not allowed!";
                return _res;
            }
            if (file.Length / 1024 > 1024)
            {
                _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                return _res;
            }
            string ext = Path.GetExtension(filename);
            byte[] filecontent = null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                filecontent = ms.ToArray();
            }

            if (!Validate.O.IsFileAllowed(filecontent, ext))
            {
                _res.Msg = "Invalid File Format!";
                return _res;
            }
            try
            {
                if (!Directory.Exists(DOCType.EmployeeGiftImgPath))
                {
                    Directory.CreateDirectory(DOCType.EmployeeGiftImgPath);
                }
                string[] Extensions = { ".png", ".jpeg", ".jpg" };
                foreach (string s in Extensions)
                {
                    string ExistFile = DOCType.EmployeeGiftImgPath + filename.Split('.')[0] + s;
                    if (File.Exists(ExistFile))
                    {
                        File.Delete(ExistFile);
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(DOCType.EmployeeGiftImgPath);
                sb.Append(filename);
                using (FileStream fs = File.Create(sb.ToString()))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = "Image uploaded successfully";
            }
            catch (Exception ex)
            {
                _res.Msg = "Error in Image uploading. Try after sometime...";
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UploadLogo",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public IResponseStatus uploadTinyMCEImage(IFormFile file, int WID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (file == null)
            {
                _res.Msg = "File Cannot be null";
                return _res;
            }
            if (!file.ContentType.Any())
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (file.Length < 1)
            {
                _res.Msg = "Empty file not allowed!";
                return _res;
            }
            if (file.Length / 1024 > 1024)
            {
                _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                return _res;
            }
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string ext = Path.GetExtension(filename);
            byte[] filecontent = null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                filecontent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(filecontent, ext))
            {
                _res.Msg = "Invalid File Format!";
                return _res;
            }
            try
            {
                StringBuilder sb = new StringBuilder(DOCType.TinyMCEImage);
                sb.Replace("{WID}", WID.ToString());
                if (!Directory.Exists(sb.ToString()))
                {
                    Directory.CreateDirectory(sb.ToString());
                }
                sb.Append(filename);
                using (FileStream fs = File.Create(sb.ToString()))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = "Image uploaded successfully";
                StringBuilder ImgPath = new StringBuilder(DOCType.TinyMCEImagePath);
                ImgPath.Replace("{WID}", WID.ToString());
                ImgPath.Append(filename);
                _res.CommonStr = "/" + ImgPath.ToString();
            }
            catch (Exception ex)
            {
                _res.Msg = "Error in Image uploading. Try after sometime...";
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UploadPopup",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public ResponseStatus UploadEmployeeImage(IFormFile file, LoginResponse _lr, string Mobile, string ImageType)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (file == null)
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (!file.ContentType.Any())
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (file.Length < 1)
            {
                _res.Msg = "Empty file not allowed!";
                return _res;
            }
            if (file.Length / 1024 > 1024)
            {
                _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                return _res;
            }
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string ext = Path.GetExtension(filename);
            byte[] filecontent = null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                filecontent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(filecontent, ext))
            {
                _res.Msg = "Invalid File Format!";
                return _res;
            }
            try
            {
                string ImgUrl = "/" + Mobile + ".png";
                StringBuilder sb = new StringBuilder();
                sb.Append(DOCType.Employee);
                if (ImageType.Equals(FolderType.ShopImage))
                {
                    sb.Append(FolderType.ShopImage);
                    sb.Append(ImgUrl);
                }

                using (FileStream fs = File.Create(sb.ToString()))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = "Image uploaded successfully";
            }
            catch (Exception ex)
            {
                _res.Msg = "Error in Image uploading. Try after sometime...";
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UploadLogo",
                    Error = ex.Message,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public IResponseStatus UploadDTHLeadOperator(IFormFile file, string WID, int OpType, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerOperatorPath.Replace("{0}", WID.ToString()).Replace("{1}", OpType.ToString()));
                    //sb.Append(DOCType.BannerUserPath.Replace("{1}", OpType.ToString()));
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append("\\");
                    sb.Append(DateTime.Now.ToString("yyyyMMddHmsfff"));
                    sb.Append(ext);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Operator Banner uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadDTHLeadOperator",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IEnumerable<BannerImage> GetOperatorBanner(string WID, int Name)
        {
            var root = Path.Combine(DOCType.BannerOperatorPath.Replace("{0}", WID).Replace("{1}", Name.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            if (Directory.Exists(root))
            {
                return Directory.EnumerateFiles(root).Select(x => new BannerImage
                {
                    FileName = Path.GetFileName(x),
                    ResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.BannerOperatorSuffix.Replace("{0}", WID).Replace("{1}", Name.ToString()), Path.GetFileName(x)).ToString(),
                    SiteResourceUrl = new StringBuilder().AppendFormat(DOCType.BannerOperatorSuffix.Replace("{0}", WID).Replace("{1}", Name.ToString()), Path.GetFileName(x)).ToString(),
                    Entrydate = File.GetCreationTime(x)
                }).OrderByDescending(x => x.Entrydate);
            }
            else
            {
                return new List<BannerImage>();
            }
        }
        public IResponseStatus RemoveOperatorBanners(string FileName, int Name, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Banner not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BannerOperatorPath.Replace("{0}", WID.ToString()).Replace("{1}", Name.ToString()));
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Banner removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        #region  AppLogoUrl
        public string GetSavedAppImage(int WID)
        {
            var root = Path.Combine(DOCType.AppImagePath.Replace("{0}", WID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            var imgData = Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                SiteFileName = Path.GetFileName(x),

                SiteResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.AppImageSuffix.Replace("{0}", WID.ToString()), Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderByDescending(x => x.Entrydate);
            string applogourl = string.Empty;
            foreach (var item in imgData)
            {
                var Lower = item.SiteResourceUrl.ToLower();
                if (Lower.Contains("app-logo"))
                {
                    applogourl = item.SiteResourceUrl;
                    break;
                }

            }
            return applogourl;
        }
        #endregion
        #region RefferalImageUploadAndRemoveSection
        public IResponseStatus RefferalImageRemove(string FileName, string WID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Refferal Image not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.RefferalContentPath.Replace("{0}", WID.ToString()));
                    sb.Append("\\");
                    sb.Append(FileName);
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Refferal Image  removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in banner removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveBanners",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public IEnumerable<BannerImage> GetSavedRefferalImage(string WID)
        {
            var root = Path.Combine(DOCType.RefferalContentPath.Replace("{0}", WID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();
            return Directory.EnumerateFiles(root).Select(x => new BannerImage
            {
                SiteFileName = Path.GetFileName(x),

                SiteResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, DOCType.RefferalContentSufix.Replace("{0}", WID.ToString()), Path.GetFileName(x)).ToString(),
                Entrydate = File.GetCreationTime(x)
            }).OrderByDescending(x => x.Entrydate);
        }
        public IResponseStatus SavedRefferalImage(IFormFile file, string WID, int OpType, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                var extUpper = ext.ToUpper();
                if (extUpper.Contains(".JPEG") || extUpper.Contains(".JPG") || extUpper.Contains(".PNG"))
                {
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.RefferalContentPath.Replace("{0}", WID.ToString()).Replace("{1}", OpType.ToString()));
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }
                        sb.Append("\\");
                        sb.Append(DateTime.Now.ToString("yyyyMMddHmsfff"));
                        sb.Append(ext);
                        using (FileStream fs = File.Create(sb.ToString()))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "RefferalImage uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        _res.Msg = "Error in RefferalImage uploading. Try after sometime...";
                        ErrorLog errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "UploadB2CBanners",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }

                }
                else
                {
                    _res.Msg = "Please Upload only 'JPG','JPEG','PNG' Images";
                    return _res;
                }
            }
            return _res;
        }

        public IResponseStatus LoaderBannerUpload(IFormFile file, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                var extUpper = ext.ToUpper();
                if (extUpper.Contains(".JPG"))
                {
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.HotelLoaderContentPath);
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }
                        sb.Append("\\");
                        string Savefilename = !File.Exists(DOCType.HotelLoaderContentPath + "/b.jpg") ? "b" : !File.Exists(DOCType.HotelLoaderContentPath + "/b1.jpg") ? "b1" : !File.Exists(DOCType.HotelLoaderContentPath + "/b2.jpg") ? "b2" : "return";
                        if (Savefilename.Contains("return"))
                        {
                            _res.Msg = "Max 3 Files Uploaded.";
                            return _res;
                        }
                        sb.Append(Savefilename);
                        sb.Append(ext);
                        using (FileStream fs = File.Create(sb.ToString()))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Hotel Image uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        _res.Msg = "Error in Hotel Image uploading. Try after sometime...";
                        ErrorLog errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "SaveHotelImage",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }

                }
                else
                {
                    _res.Msg = "Please Upload only 'JPG' Images";
                    return _res;
                }
            }
            return _res;
        }
        
        public IResponseStatus SaveHotelImage(IFormFile file,  LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                var extUpper = ext.ToUpper();
                if (extUpper.Contains(".JPG"))
                {
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DOCType.HotelContentPath);
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());

                        }
                        sb.Append("\\");
                        sb.Append(OPTypes.Hotel);
                        sb.Append(ext);
                        using (FileStream fs = File.Create(sb.ToString()))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Hotel Image uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        _res.Msg = "Error in Hotel Image uploading. Try after sometime...";
                        ErrorLog errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "SaveHotelImage",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }

                }
                else
                {
                    _res.Msg = "Please Upload only 'JPG' Images";
                    return _res;
                }
            }
            return _res;
        }
        #endregion
        public ResponseStatus UploadBankBanner(IFormFile file, int OID)
        {
            var res = Validate.O.IsImageValid(file);
            if (res.Statuscode == ErrorCodes.One)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.BankBannerPath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(OID.ToString());
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadOperatorIcon",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }

        public ResponseStatus UploadCouponVoucher(IFormFile file,int OID,string Filename)
        {
            var res = Validate.O.IsImageValid(file);
            if (res.Statuscode == ErrorCodes.One)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.CouponVoucherPath.Replace("{0}",OID.ToString()));

                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(OID+"_"+Filename);
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadCouponVoucher",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }
        public IResponseStatus RemoveCouponVoucher(string OID, string FileName, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((FileName ?? "") == "")
                {
                    _res.Msg = "Invalid Request!";
                    return _res;
                }
                _res.Msg = "Voucher not exists!";
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.CouponVoucherPath.Replace("{0}", OID.ToString()));
                    //sb.Append("\\");
                    sb.Append(OID+"_"+FileName+".png");
                    if (File.Exists(sb.ToString()))
                    {
                        File.Delete(sb.ToString());
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Voucher removed successfully!";
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Voucher removal!";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "RemoveCouponVoucher",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public IrctcCertificateModel downloadPdfirctc(int LT, int UserID, int WID)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = LT,
                LoginID = UserID
            };
            IWebsiteML ml = new WebsiteML(_accessor, _env);
                          
           
            string _certDocName = DOCType.WebsiteFolderPath.Replace("{0}", WID.ToString());
            
            IProcedure proc = new ProcIrctcPdfData(_dal);
            IrctcCertificateModel result = (IrctcCertificateModel)proc.Call(req);
            IProcedure procC = new ProcCompanyProfile(_dal);
            var companyDetail = (CompanyProfileDetail)procC.Call(WID);
            result.CompanyName = companyDetail.Name;
            result.URl= ml.GetWebsiteInfo().AbsoluteHost;
            result.BgImage = DOCType.IrctcCertificateBackGroundSign.Replace('\\', '/').Replace("{0}", WID.ToString());
            result.LogoImage = DOCType.Logo.Replace("{0}", WID.ToString());
            result.SignImage = DOCType.CertificateImgSign.Replace("{0}", WID.ToString());
           
            return result;
        }

        public IrctcCertificateModel GetOutletData(int LT, int UserID)
        {

            CommonReq req = new CommonReq
            {
                LoginTypeID = LT,
                LoginID = UserID
            };
            IProcedure proc = new ProcGetOutLetData(_dal);
            IrctcCertificateModel result = (IrctcCertificateModel)proc.Call(req);
            
            return result;
        }

        public IResponseStatus UploadAdvertisementImage(IFormFile file,int LT, int UserID,string imageName)
        { 
           
            var res = Validate.O.IsImageValid(file);
            if (res.Statuscode == ErrorCodes.One)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.AdvertisementImagePath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    sb.Append(imageName);
                    sb.Append(".png");
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadAdvertisementImage",
                        Error = ex.Message,
                        LoginTypeID =LT,
                        UserId = UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }




    }
}
