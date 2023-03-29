using SignalR.SmartCore.Server.DependencyInjections;

namespace SignalR.SmartCore.Server
{
    /// <summary>
    /// Global parameters
    /// </summary>
    internal static class SmartGlobal
    {
        private static IEnumerable<Type>? _smartHubConcreteTypes;

        internal static IEnumerable<Type> SmartHubConcreteTypes
        {
            get
            {
                _smartHubConcreteTypes ??= AppDomain.CurrentDomain
                                                      .GetAssemblies()
                                                      .SelectMany(s => s.GetTypes())
                                                      .Where(type => type.IsConcreteAndSubClassOfSmartHub());
                return _smartHubConcreteTypes;
            }
        }
    }
}
