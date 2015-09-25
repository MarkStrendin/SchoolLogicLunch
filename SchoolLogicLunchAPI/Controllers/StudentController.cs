using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SchoolLogicLunchAPI.Repositories;
using SLData;

namespace SchoolLogicLunchAPI.Controllers
{
    public class StudentController : ApiController
    {
        static readonly StudentRepository Repository = new StudentRepository();


        public IEnumerable<Student> Get()
        {
            return Repository.GetAll();
        } 
    }
}
