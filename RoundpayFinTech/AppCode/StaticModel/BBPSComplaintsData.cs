using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.StaticModel
{
    public class BBPSComplaintsData
    {
        public static string[] ReasonTransactionLevel
        {
            get
            {
                return new string[]
                {
                    "Transaction Successful, account not updated", "Amount deducted, biller account credited but transaction ID not received",
                    "Amount deducted, biller account not credited & transaction ID not received","Amount deducted multiple times","Double payment updated",
                    "Erroneously paid in wrong account","Others, provide details in description"
                };
            }
        }

        public static string[] ReasonServiceAgent
        {
            get
            {
                return new string[]
                {
                    "Agent not willing to print receipt","Agent misbehaved","Agent outlet closed","Agent denying registration of complaint","Agent not accepting certain bills",
                    "Agent overcharging"
                };
            }
        }

        public static string[] ReasonServiceBiller
        {
            get
            {
                return new string[]
                {
                    "Biller available. Unable to transact","Multiple failure for same biller","Denomination not available","Incorrect bill details displayed",
                    "Incomplete / No details reflecting"
                };
            }
        }
    }
}
