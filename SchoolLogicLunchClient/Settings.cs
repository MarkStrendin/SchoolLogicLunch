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
        public static bool IsConfigFileValid()
        {
            if (
                (!string.IsNullOrEmpty(Settings.ServerURL)) &&
                (Settings.MealType > 0) && 
                (Settings.SchoolDatabaseID > 0)
                )
            {
                return true;
            }
            return false;
        }

        public static int SchoolDatabaseID
        {
            get
            {
                return Parsers.ParseInt(ConfigurationManager.AppSettings["iSchoolID"]);
            }
        }

        public static int MealType
        {
            get
            {
                return Parsers.ParseInt(ConfigurationManager.AppSettings["MealTypeID"]);
            }
        }

        public static string ServerURL
        {
            get {
                return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["APIURL"]) ? ConfigurationManager.AppSettings["APIURL"] : string.Empty;
            }
        }

        public static bool AllowReducedMeals
        {
            get
            {
                return Parsers.ParseBool(ConfigurationManager.AppSettings["AllowReducedPrice"]);
            }
        }

        public static bool AllowFreeMeals
        {
            get
            {
                return Parsers.ParseBool(ConfigurationManager.AppSettings["AllowFreeMeals"]);
            }
        }

    }
}
