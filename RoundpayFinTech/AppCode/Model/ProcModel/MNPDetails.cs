using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
	public class MNPUser
	{


		public int ID { get; set; }
		public int UserID { get; set; }
		public string Name { get; set; }
		public string MobileNo { get; set; }
		public int VerifyStatus { get; set; }
		public string Remark { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Operatorname { get; set; }
		public string ReferenceID { get; set; }
		public decimal Amount { get; set; }
		public string DemoInput { get; set; }
		public string FRCDate { get; set; }
		public string FRCType { get; set; }
		public string FRCDemoNumber { get; set; }
		public string FRCDoneDate { get; set; }
		public string RegistrationDate { get; set; }
		public string MobileMNP { get; set; }
		public string CutomerMObile { get; set; }
		public string ClaimAmountDate { get; set; }

	}


	public class MNPUserResp
	{
		public int Statuscode { get; set; }
		public string Msg { get; set; }

		public int RoleID { get; set; }
		public List<MNPUser> list { get; set; }
	}

	public class MNPDetailsResp
	{
		public int Statuscode { get; set; }
		public string Msg { get; set; }

		public int RoleID { get; set; }


		public MNPUser data { get; set; }
		public class RegisterAsChildPartnerResp
		{
			public int UserId { get; set; }
			public int Statuscode { get; set; }
			public string Msg { get; set; }
			public int RoleID { get; set; }
			public string PSAId { get; set; }
			public string FatherName { get; set; }
		}
	}
}
