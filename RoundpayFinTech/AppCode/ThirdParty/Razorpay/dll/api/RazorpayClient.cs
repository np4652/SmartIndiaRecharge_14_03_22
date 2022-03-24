using System.Collections.Generic;

namespace Razorpay.Api
{
    public class RazorpayClient
    {
        protected const string version = "3.0.0";

        protected const string baseUrl = "https://api.razorpay.com/v1/";

        protected static string key = null;

        protected static string secret = null;

        protected static List<Dictionary<string, string>> appsDetails = new List<Dictionary<string, string>>();

        protected static Dictionary<string, string> headers = new Dictionary<string, string>();

        private Payment payment;

        private Order order;

        private Refund refund;

        private Customer customer;

        private Invoice invoice;

        private Card card;

        private Transfer transfer;

        private Addon addon;

        private Plan plan;

        private Subscription subscription;

        private VirtualAccount virtualaccount;

        public static string Key
        {
            get
            {
                return key;
            }
            private set
            {
                key = value;
            }
        }

        public static string Secret
        {
            get
            {
                return secret;
            }
            private set
            {
                secret = value;
            }
        }

        public static List<Dictionary<string, string>> AppsDetails
        {
            get
            {
                return appsDetails;
            }
        }

        public static Dictionary<string, string> Headers
        {
            get
            {
                return headers;
            }
        }

        public static string BaseUrl
        {
            get
            {
                return "https://api.razorpay.com/v1/";
            }
        }

        public static string Version
        {
            get
            {
                return "3.0.0";
            }
        }

        public Payment Payment
        {
            get
            {
                if (payment == null)
                {
                    payment = new Payment("");
                }
                return payment;
            }
        }

        public Order Order
        {
            get
            {
                if (order == null)
                {
                    order = new Order();
                }
                return order;
            }
        }

        public Refund Refund
        {
            get
            {
                if (refund == null)
                {
                    refund = new Refund();
                }
                return refund;
            }
        }

        public Customer Customer
        {
            get
            {
                if (customer == null)
                {
                    customer = new Customer("");
                }
                return customer;
            }
        }

        public Invoice Invoice
        {
            get
            {
                if (invoice == null)
                {
                    invoice = new Invoice();
                }
                return invoice;
            }
        }

        public Card Card
        {
            get
            {
                if (card == null)
                {
                    card = new Card();
                }
                return card;
            }
        }

        public Transfer Transfer
        {
            get
            {
                if (transfer == null)
                {
                    transfer = new Transfer("");
                }
                return transfer;
            }
        }

        public Addon Addon
        {
            get
            {
                if (addon == null)
                {
                    addon = new Addon("");
                }
                return addon;
            }
        }

        public Plan Plan
        {
            get
            {
                if (plan == null)
                {
                    plan = new Plan();
                }
                return plan;
            }
        }

        public Subscription Subscription
        {
            get
            {
                if (subscription == null)
                {
                    subscription = new Subscription("");
                }
                return subscription;
            }
        }

        public VirtualAccount VirtualAccount
        {
            get
            {
                if (virtualaccount == null)
                {
                    virtualaccount = new VirtualAccount("");
                }
                return virtualaccount;
            }
        }

        public RazorpayClient(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }

        public void setAppsDetails(string title, string version)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("title", title);
            dictionary.Add("version", version);
            appsDetails.Add(dictionary);
        }

        public void addHeader(string key, string value)
        {
            headers.Add(key, value);
        }
    }
}
