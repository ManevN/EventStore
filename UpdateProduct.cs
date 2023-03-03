using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp
{
    [Route("update-prouct")]
    public class UpdateProduct : Controller
    {
        private readonly EventStoreContext eventStore;
        private readonly ILogger<UpdateProduct> logger;

        public UpdateProduct(EventStoreContext eventStore, ILogger<UpdateProduct> logger)        
        {
            this.eventStore = eventStore;
            this.logger = logger;
        }


        [HttpGet]
        public void UpdateProductEndpoint()
        {
            logger.LogInformation("Product is updating");

            var restul = eventStore.Events.TagWith("This is select query").ToList();
        }
    }
}
