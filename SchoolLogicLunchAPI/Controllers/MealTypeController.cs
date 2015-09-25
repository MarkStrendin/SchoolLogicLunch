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
    public class MealTypeController : ApiController
    {
        static readonly MealTypeRepository Repository = new MealTypeRepository();

        public IEnumerable<MealType> Get()
        {
            return Repository.GetAll();
        }
    }
}
