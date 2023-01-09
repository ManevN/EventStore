using Microsoft.AspNetCore.Mvc;

namespace WebApp
{
    [Route("update-prouct")]
    public class UpdateProduct : Controller
    {
        private readonly EventStoreContext eventStore;

        public UpdateProduct(EventStoreContext eventStore)
        
        {
            this.eventStore = eventStore;
        }


        [HttpGet]
        public void UpdateProductEndpoint()
        {
            var restul = eventStore.Events.ToList();
        }
    }
}
