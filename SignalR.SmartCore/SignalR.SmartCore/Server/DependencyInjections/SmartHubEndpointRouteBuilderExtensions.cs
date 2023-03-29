using Microsoft.AspNetCore.Routing;
using SignalR.SmartCore.Server;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder
{
    public static class SmartHubEndpointRouteBuilderExtensions
    {
        public static void MapSmartHub(this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));

            //map all smarthub
            var mapHubMethod = typeof(HubEndpointRouteBuilderExtensions).GetMethods().Where(m => m.Name == "MapHub" && m.IsGenericMethod).First();
            foreach (var smartHubType in ServerGlobal.SmartHubConcreteTypes)
            {
                //Dynamically generate and execute MapHub<T> methods
                var routeAttribute = smartHubType.GetCustomAttribute<HubRouteAttribute>();
                var route = routeAttribute?.RouteName ?? smartHubType.Name;

                var genericMapHubMethod = mapHubMethod.MakeGenericMethod(smartHubType);
                genericMapHubMethod.Invoke(null, new object[] { endpoints, route });
            }
        }
    }
}
