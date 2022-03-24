using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
	public class News
	{
		public int ID { get; set; }
		public string Title { get; set; }
		public string NewsDetail { get; set; }
		public int RoleId { get; set; }
		public string CreateDate { get; set; }
		public string RoleName { get; set; }
		public List<RoleMaster> Roles { get; set; }
		public List<int> Role { get; set; }        
	}

	public class UserNews 
	{		
		public List<News> ListNews { get; set; }
	}

}
