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
    public class PurchasedMealController : ApiController
    {
        static readonly PurchasedMealRepository Repository = new PurchasedMealRepository();

        // GET api/<controller>
        public IEnumerable<PurchasedMeal> Get()
        {
            return Repository.GetAll();
        }

        // GET api/<controller>/5
        public PurchasedMeal Get(int id)
        {
            return Repository.Get(id);
        }
        
        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]PurchasedMeal value)
        {
            try
            {
                // Add the meal, but also get the ID number for the meal added by the repository so the client can get that
                value = Repository.Add(value);

                // Apparently we should be responding to a POST request with HTTP status 201 instead of 200, which would be the default
                HttpResponseMessage response = Request.CreateResponse<PurchasedMeal>(HttpStatusCode.Created, value);
                return response;
            }
            catch( Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}