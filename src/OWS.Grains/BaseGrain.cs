using Orleans.Runtime;

namespace OWS.Grains
{
    public class BaseGrain : Grain
    {
        internal static Guid GetCustomerId()
        {
            if (!Guid.TryParse(RequestContext.Get("CustomerId").ToString(), out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            return customerGuid;
        }
    }
}
