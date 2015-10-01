using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using SLData;

namespace SchoolLogicLunchAPI.Repositories
{
    public class MealTypeRepository
    {

        private static Dictionary<int, MealType> _cachedMealTypes = new Dictionary<int, MealType>();
        private static DateTime _cacheLastRefreshed = DateTime.MinValue;
        private static object _mealTypeCacheLock = new object();

        private static Dictionary<int, MealType> GetCache()
        {
            if (_cachedMealTypes == null)
            {
                _cachedMealTypes = new Dictionary<int, MealType>();
            }

            lock (_mealTypeCacheLock)
            {
                if (DateTime.Now.Subtract(_cacheLastRefreshed) > new TimeSpan(0, 10, 0)) // Cache for 10 minutes
                {
                    _cachedMealTypes = new Dictionary<int, MealType>();

                    using (SqlConnection connection = new SqlConnection(Settings.DatabaseConnectionString))
                    {
                        SqlCommand sqlCommand = new SqlCommand
                        {
                            Connection = connection,
                            CommandType = CommandType.Text,
                            CommandText = "SELECT * FROM MealType;"
                        };

                        sqlCommand.Connection.Open();
                        SqlDataReader dataReader = sqlCommand.ExecuteReader();

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                MealType foundMealType = dataReaderToMealType(dataReader);
                                if (foundMealType != null)
                                {
                                    _cachedMealTypes.Add(foundMealType.ID, foundMealType);
                                }
                            }
                        }

                        sqlCommand.Connection.Close();
                    }

                    _cacheLastRefreshed = DateTime.Now;
                }
            }

            return _cachedMealTypes;
        }

        public static Dictionary<int, MealType> GetDictionary()
        {
            return GetCache();
        }

        public IEnumerable<MealType> GetAll()
        {
            return GetCache().Values.ToList();
        }

        private static MealType dataReaderToMealType(SqlDataReader dataReader)
        {
            return new MealType()
            {
                ID = Parsers.ParseInt(dataReader["iMealTypeID"].ToString().Trim()),
                AllowedFree = !Parsers.ParseBool(dataReader["lDisallowFree"].ToString().Trim()),
                BarcodeValue = dataReader["cBarcodeNumber"].ToString().Trim(),
                FreeAmount = Parsers.ParseDecimal(dataReader["nFreeAmount"].ToString().Trim()),
                FullAmount = Parsers.ParseDecimal(dataReader["nFullAmount"].ToString().Trim()),
                iSchoolID = Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                Name = dataReader["cName"].ToString().Trim(),
                ReducedAmount = Parsers.ParseDecimal(dataReader["nReducedAmount"].ToString().Trim()),
                SortOrder = Parsers.ParseInt(dataReader["iOrder"].ToString().Trim())
            };
        }
    }
}