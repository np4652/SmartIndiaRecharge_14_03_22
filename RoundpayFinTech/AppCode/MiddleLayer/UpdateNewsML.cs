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
namespace RoundpayFinTech.AppCode.MiddleLayer
{
	public class UpdateNewsML : IUpdateNewsML
	{
		private readonly IHttpContextAccessor _accessor;
		private readonly IHostingEnvironment _env;
		private readonly ISession _session;
		private readonly IDAL _dal;
		private readonly IConnectionConfiguration _c;
		private readonly LoginResponse _lr;

		public UpdateNewsML(IHttpContextAccessor accessor, IHostingEnvironment env)
		{
			_accessor = accessor;
			_env = env;
			_c = new ConnectionConfiguration(_accessor, _env);
			_session = _accessor.HttpContext.Session;
			_dal = new DAL(_c.GetConnectionString());
			_lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
		}

		public IResponseStatus UpdateNews(News newsReq)
		{
			IResponseStatus _resp = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			if (_lr.RoleID.In(Role.Admin,Role.MasterWL) && _lr.LoginTypeID == LoginType.ApplicationUser)
			{
				string Role = "";
				for (int i = 0; i < newsReq.Role.Count; i++)
				{
					if (i == 0)
						Role += newsReq.Role[i];
					else
						Role += "," + newsReq.Role[i];
				}

				CommonReq commonReq = new CommonReq
				{
					LoginID = _lr.UserID,
					LoginTypeID = _lr.LoginTypeID,
					CommonInt = newsReq.ID,
					str = newsReq.NewsDetail,
					CommonStr = newsReq.Title,
					CommonInt2 = _lr.WID,
					CommonStr2 = Role
			};

				IProcedure _proc = new ProcUpdateNews(_dal);
                _resp= (ResponseStatus)_proc.Call(commonReq);

			}
			return _resp;
		}
	}
}
