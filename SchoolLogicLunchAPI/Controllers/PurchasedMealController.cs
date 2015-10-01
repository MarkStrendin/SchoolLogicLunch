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
                Dictionary<int, MealType> allMealTypes = MealTypeRepository.GetDictionary();

                // Find the selected meal type
                if (allMealTypes.ContainsKey(value.MealType))
                {
                    MealType selectedMealType = allMealTypes[value.MealType];

                    if (value.Amount <= selectedMealType.FullAmount)
                    {
                        if (value.Amount >= selectedMealType.FullAmount*-1)
                        {
                            value = Repository.Add(value);

                            // Apparently we should be responding to a POST request with HTTP status 201 instead of 200, which would be the default
                            HttpResponseMessage response = Request.CreateResponse<PurchasedMeal>(HttpStatusCode.Created, value);
                            return response;
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Amount cannot be less than full price * -1"));  
                        }
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Amount cannot be more than full price"));  
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new Exception("Invalid MealID"));    
                }
            }
            catch( Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}