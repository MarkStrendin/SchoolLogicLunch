using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolLogicLunchClient
{
    static class Settings
    {
        public static int SchoolDatabaseID
        {
            get
            {
                //return Parsers.ParseInt(ConfigurationManager.AppSettings["SchoolDatabaseID"].ToString().Trim());
                return 0;
            }
        }

        public static int MealType
        {
            get
            {
                //return Parsers.ParseInt(ConfigurationManager.AppSettings["MealTypeID"].ToString().Trim());
                return 1001;
            }
        }

        public static string ServerURL
        {
            get
            {
                //return ConfigurationManager.AppSettings["ServerURL"].ToString().Trim();
                return "https://sldata.lskysd.ca/SchoolLogicLunch/";
            }
        }

        public static bool AllowFreeMeals
        {
            get { return false; }
        }

    }
}
