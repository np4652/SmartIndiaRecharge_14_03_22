using System.Collections.Generic;

namespace Fintech.AppCode.StaticModel
{
    public class State
    {
        public static List<string> GetStates()
        {
            List<string> states = new List<string>() {
                "Andaman and Nicobar Islands : 35","Andhra Pradesh : 28","Andhra Pradesh(New) : 37","Arunachal Pradesh : 12","Assam : 18","Bihar : 10","Chandigarh : 04","Chattisgarh : 22","Dadra and Nagar Haveli : 26","Daman and Diu : 25","Delhi : 07","Goa : 30","Gujarat : 24","Haryana : 06","Himachal Pradesh : 02","Jammu and Kashmir : 01","Jharkhand : 20","Karnataka : 29","Kerala : 32","Lakshadweep Islands : 31","Madhya Pradesh : 23","Maharashtra : 27","Manipur : 14","Meghalaya : 17","Mizoram : 15","Nagaland : 13","Odisha : 21","Pondicherry : 34","Punjab : 03","Rajasthan : 08","Sikkim : 11","Tamil Nadu : 33","Telangana : 36","Tripura : 16","Uttar Pradesh : 09","Uttarakhand : 05","West Bengal : 19"
            };
            return states;
        }
    }
}
 
