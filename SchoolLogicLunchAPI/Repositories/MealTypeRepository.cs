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
        public IEnumerable<MealType> GetAll()
        {
            List<MealType> returnMe = new List<MealType>();

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
                            returnMe.Add(foundMealType);
                        }
                    }
                }

                sqlCommand.Connection.Close();
            }
            return returnMe;
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