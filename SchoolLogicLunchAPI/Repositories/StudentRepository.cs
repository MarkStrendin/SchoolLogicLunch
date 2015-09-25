using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using SLData;

namespace SchoolLogicLunchAPI.Repositories
{
    public class StudentRepository
    {
        private static Student dataReaderToStudent(SqlDataReader dataReader)
        {
            return new Student()
            {
                Bank = 0,
                FirstName = dataReader["cFirstName"].ToString().Trim(),
                LastName = dataReader["cLastName"].ToString().Trim(),
                ID = Parsers.ParseInt(dataReader["iStudentID"].ToString().Trim()),
                MedicalNotes = dataReader["mMedical"].ToString().Trim(),
                StudentNumber = dataReader["cStudentNumber"].ToString().Trim()
            };
        }

        public IEnumerable<Student> GetAll()
        {
            List<Student> returnMe = new List<Student>();

            using (SqlConnection connection = new SqlConnection(Settings.DatabaseConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText =
                        "SELECT iStudentID, cStudentNumber, cFirstName, cLastName, iSchoolID, mMedical FROM Student"
                };

                sqlCommand.Connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Student foundStudent = dataReaderToStudent(dataReader);
                        if (foundStudent != null)
                        {
                            if (!string.IsNullOrEmpty(foundStudent.StudentNumber.Trim()))
                            {
                                returnMe.Add(foundStudent);
                            }
                        }
                    }
                }

                sqlCommand.Connection.Close();
            }
            // Get the student meal information to figure out how much money the student has banked
            // TODO

            return returnMe;
        } 
    }
}