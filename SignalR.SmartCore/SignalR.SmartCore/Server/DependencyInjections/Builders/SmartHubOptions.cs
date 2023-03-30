using SignalR.SmartCore.Server.Filters.SmartHubFilters;

namespace SignalR.SmartCore.Server.DependencyInjections.Builders
{
    public class SmartHubOptions
    {
        internal List<Type> FilterTypes = new List<Type>();

        public void AddFilter<TFilter>() where TFilter: ISmartHubFilterBase
        {
            AddFilter(typeof(TFilter));
        }

        public void AddFilter(Type filterType)
        {
            if (!typeof(ISmartHubFilterBase).IsAssignableFrom(filterType)) throw new Exception("filterType must be an implementation of ISmartHubFilterBase");
            FilterTypes.Add(filterType);
        }
    }
}
