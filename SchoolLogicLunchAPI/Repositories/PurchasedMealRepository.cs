using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SLData;

namespace SchoolLogicLunchAPI.Repositories
{
    public class PurchasedMealRepository
    {
        public IEnumerable<PurchasedMeal> GetAll()
        {
            List<PurchasedMeal> returnedMeals = new List<PurchasedMeal>();

            using (SqlConnection connection = new SqlConnection(Settings.DatabaseConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText =
                        "SELECT iStudentMealID, iStudentID, iMealTypeID, dDate, nAmount, iSchoolID FROM StudentMeal ORDER BY iStudentMealID DESC;"
                };

                sqlCommand.Connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        returnedMeals.Add(dataReaderToPurchasedMeal(dataReader));
                    }
                }

                sqlCommand.Connection.Close();
            }
            return returnedMeals;
        }

        private static PurchasedMeal dataReaderToPurchasedMeal(SqlDataReader dataReader)
        {
            return new PurchasedMeal()
            {
                MealID = Parsers.ParseInt(dataReader["iStudentMealID"].ToString().Trim()),
                StudentID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                MealType = Parsers.ParseInt(dataReader["iMealTypeID"].ToString().Trim()),
                Amount = Parsers.ParseDecimal(dataReader["nAmount"].ToString().Trim()),
                SchoolID =Parsers.ParseInt(dataReader["iSchoolID"].ToString().Trim()),
                DateAndTime = Parsers.ParseDate(dataReader["dDate"].ToString().Trim())
            };
        }

        public PurchasedMeal Get(int id)
        {
            PurchasedMeal returnedMeal = new PurchasedMeal();

            using (SqlConnection connection = new SqlConnection(Settings.DatabaseConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText =
                        "SELECT iStudentMealID, iStudentID, iMealTypeID, dDate, nAmount, iSchoolID FROM StudentMeal WHERE iStudentMealID=@MEALID ORDER BY iStudentMealID DESC;"
                };
                sqlCommand.Parameters.AddWithValue("MEALID", id);
                sqlCommand.Connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        returnedMeal = dataReaderToPurchasedMeal(dataReader);
                    }
                }

                sqlCommand.Connection.Close();
            }
            return returnedMeal;
        }

        public PurchasedMeal Add(PurchasedMeal newMeal)
        {
            // Maybe do each of these in a different thread, so the web service is nice and speedy
            // if people click quickly.

            using (SqlConnection connection = new SqlConnection(Settings.DatabaseConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText =
                        "INSERT INTO StudentMeal(iStudentID, iMealTypeID, dDate, nAmount, iSchoolID) VALUES(@STUDENTID, @MEALTYPE, @CURDATE, @AMOUNT, @SCHOOLID); "
                };

                sqlCommand.Parameters.AddWithValue("STUDENTID", newMeal.StudentID);
                sqlCommand.Parameters.AddWithValue("MEALTYPE", newMeal.MealType);
                sqlCommand.Parameters.AddWithValue("CURDATE", DateTime.Now);
                sqlCommand.Parameters.AddWithValue("AMOUNT", newMeal.Amount);
                sqlCommand.Parameters.AddWithValue("SCHOOLID", newMeal.SchoolID);

                sqlCommand.Connection.Open();
                newMeal.MealID = sqlCommand.ExecuteNonQuery();
                sqlCommand.Connection.Close();
            }

            return newMeal;
        }

    }
}