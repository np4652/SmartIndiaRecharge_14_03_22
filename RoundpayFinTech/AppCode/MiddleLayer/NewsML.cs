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
using System.Collections.Generic;
using System.Linq;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
	public class NewsML : INewsML, INewsAppML
    {
		private readonly IHttpContextAccessor _accessor;
		private readonly IHostingEnvironment _env;
		private readonly ISession _session;
		private readonly IDAL _dal;
		private readonly IConnectionConfiguration _c;
		private readonly LoginResponse _lr;

		public NewsML(IHttpContextAccessor accessor, IHostingEnvironment env,bool IsInSession=true)
		{
			_accessor = accessor;
			_env = env;
			_c = new ConnectionConfiguration(_accessor, _env);
			_dal = new DAL(_c.GetConnectionString());
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
		}
		public UserNews GetNews()
		{
            //List<News> setting = new List<News>();
            IUserML userML = new UserML(_lr);
			var model = new UserNews();
			if (_lr.RoleID.In(Role.Admin,Role.MasterWL) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditNews))
			{
				IProcedure _proc = new ProcNews(_dal);
				var commonReq = new CommonReq
				{
					LoginID = _lr.UserID,
					LoginTypeID = _lr.LoginTypeID,
					CommonInt = 0,
					CommonInt2 = _lr.WID
				};
				model = (UserNews)_proc.Call(commonReq);
			}
			return model;
		}
		public News EditNews(int ID)
		{
			var model = new News();
			if (_lr.RoleID.In(Role.Admin,Role.MasterWL)  && _lr.LoginTypeID == LoginType.ApplicationUser)
			{
				var commonReq = new CommonReq
				{
					CommonInt = ID,
					CommonInt2 = _lr.RoleID
				};
				IProcedure _proc = new ProcGetNewsByID(_dal);
				model = (News)_proc.Call(commonReq);
				IProcedure proc = new FnGetChildRolesForNews(_dal);
				model.Roles = (List<RoleMaster>)proc.Call(commonReq);
			}
			return model;
		}
		public UserNews GetNewsRoleAssign(int ID)
		{
			var model = new UserNews();
			if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
			{
				CommonReq commonReq = new CommonReq
				{
					LoginID = _lr.UserID,
					LoginTypeID = _lr.LoginTypeID,
					CommonInt = ID,
					CommonInt3 = 1
				};

				IProcedure _proc = new ProcGetAsignNews(_dal);
				model = (UserNews)_proc.Call(commonReq);
			}
			return model;
		}
		public News AddNews()
		{
			News model = new News();
			if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
			{
				CommonReq commonReq = new CommonReq
				{
					LoginID = _lr.UserID,
					LoginTypeID = _lr.LoginTypeID,

				};
				IProcedure proc = new FnGetChildRoles(_dal);
				model.Roles = (List<RoleMaster>)proc.Call(_lr.RoleID);
			}
			return model;
		}

		public IResponseStatus CallAddNews(News reqData)
		{
			var res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			News model = new News();
			if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
			{
				string Role = "";
				CommonReq commonReq = new CommonReq();

				for (int i = 0; i < reqData.Role.Count; i++)
				{
					if (i == 0)
						Role += reqData.Role[i];
					else
						Role += "," + reqData.Role[i];
				}
				commonReq.LoginID = _lr.UserID;
				commonReq.LoginTypeID = _lr.LoginTypeID;
				commonReq.str = reqData.NewsDetail;
				commonReq.CommonStr = reqData.Title;
				commonReq.CommonStr2 = Role;
				commonReq.CommonInt = _lr.WID;
				IProcedure _proc = new ProcAddNews(_dal);
				_proc.Call(commonReq);

				res.Statuscode = ErrorCodes.One;
				res.Msg = ErrorCodes.SUCCESS;

			}
			return res;
		}
		public IResponseStatus DeleteNews(CommonReq reqdata)
		{
			IResponseStatus res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			News model = new News();
			if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser)
			{
				IProcedure _proc = new ProcDeleteNews(_dal);
				return (IResponseStatus)_proc.Call(reqdata);

			}
			return res;
		}
		public List<News> GetNewsByRole(int RoleId)
		{
			var model = new List<News>();
			if (_lr.LoginTypeID == LoginType.ApplicationUser)
			{
				var commonReq = new CommonReq
				{
					LoginID = _lr.UserID,
					LoginTypeID = _lr.LoginTypeID,
					CommonInt3 = RoleId,
					CommonInt4 = _lr.WID
				};
				IProcedure _proc = new ProcGetNews(_dal);
				model = (List<News>)_proc.Call(commonReq);
			}
			return model;
		}
        public News GetNewsRoleNewsForApp(CommonReq commonReq)
        {          
            IProcedure _proc = new ProcGetNewsForLogin(_dal);
            var model=(List<News>)_proc.Call(commonReq);
            string NewsDetail = "";
            if (model.Any())
            {
                NewsDetail = string.Join(" || ", model.Select(x => x.NewsDetail)).Replace("</p> || <p>"," || ");
            }
            return new News { ID=1,NewsDetail=NewsDetail,Title=string.Empty,RoleId=commonReq.CommonInt3};
        }
    }
}
