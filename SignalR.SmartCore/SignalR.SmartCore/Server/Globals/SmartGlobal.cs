using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server
{
    public static class SmartGlobal
    {
        private static IEnumerable<Type>? _smartHubConcreteTypes;

        public static IEnumerable<Type> SmartHubConcreteTypes
        {
            get
            {
                if (_smartHubConcreteTypes == null)
                {
                    _smartHubConcreteTypes = AppDomain.CurrentDomain
                                                      .GetAssemblies()
                                                      .SelectMany(s => s.GetTypes())
                                                      .Where(type => !type.IsAbstract &&
                                                             type.IsSubclassOf(typeof(Hub)));
                }
#pragma warning disable CS8603 // 可能有 Null 參考傳回。
                return _smartHubConcreteTypes;
#pragma warning restore CS8603 // 可能有 Null 參考傳回。
            }
        }
    }
}
