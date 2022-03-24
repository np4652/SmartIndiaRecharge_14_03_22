using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
	public class JWTReqUsers
	{
		public int UserID { get; set; }
		public string UserToken { get; set; }
	}

	public class JWTTokensResp
	{
		public int StatusCode { get; set; }
		public string Msg { get; set; }
		public string Token { get; set; }
	}

	public class JwtTokenSession : JWTReqUsers
	{ 
		public string UserJWTToken { get; set; }

	}
}
