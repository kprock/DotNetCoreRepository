using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository
{
    public class Constants
    {
        public static class MWSAPI
        {
            // MWSAuthToken, AWS Access Key ID
            public static string AWS_ACCESS_KEY_ID = "";

            // Developer AWS secret key
            public static string SECRET_KEY = "";

            // Client application name
            public static string APP_NAME = "";

            // Client application version
            public static string APP_VERSION = "1.0";

            // Merchant ID, Merchant Token, Seller ID
            public static string SELLER_ID = "";

            // Marketplace ID
            public static string MARKETPLACE_ID = "";

            // The endpoint for region service and version
            public static string SERVICE_URL = "https://mws.amazonservices.com";
        }

        public static class SapDiapiSettings
        {
            // move to environment variables
            public static string B1_SERVER = "";
            public static string B1_LICENSE_SERVER = "192.168.1.1";
            public static string B1_DB_USER_NAME = "adm1";
            public static string B1_DB_PASSWORD = "pw1";
            public static string B1_COMPANY_DB = "Db";
            public static string B1_SANDBOX_DB = "SandboxDb";
            public static string B1_USER_NAME = "sapAdm";
            public static string B1_PASSWORD = "sapPw1";
            public static string B1_TESTMODE = "false";
            public static string NEXT_CARDCODE_QUERY = "";
            public static string NEXT_ORDER_DOCNUM_QUERY = "";
        }
    }
}
