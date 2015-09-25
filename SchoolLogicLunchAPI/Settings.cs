using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SchoolLogicLunchAPI
{
    public static class Settings
    {
        public static string DatabaseConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["SchoolLogicDatabase"].ToString().Trim();
            }
        }


    }
}