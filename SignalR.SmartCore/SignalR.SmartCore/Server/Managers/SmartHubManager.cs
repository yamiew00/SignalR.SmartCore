using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalR.SmartCore.Server.Managers
{
    /// <summary>
    /// The SmartHubManager class manages SmartHub instances, adding them on user connect successfully
    /// and removing them on user disconnection. It is designed as a singleton for the application.
    /// </summary>
    internal class SmartHubManager : ISmartHubManager
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Hub>> Dictionary;

        internal SmartHubManager(IEnumerable<Type> hubTypes)
        {
            Dictionary = new ConcurrentDictionary<Type, ConcurrentDictionary<string, Hub>>();
            foreach (var hubType in hubTypes)
            {
                Dictionary.TryAdd(hubType, new ConcurrentDictionary<string, Hub>());
            }
        }

        public IEnumerable<Hub> AllClients => Dictionary.SelectMany(dicts => dicts.Value)
                                                        .Select(item => item.Value)
                                                        .ToList();

        public IEnumerable<T> Clients<T>() where T : Hub
        {
            if (!Dictionary.TryGetValue(typeof(T), out var dict)) return Enumerable.Empty<T>();
            return dict.Values.Cast<T>().ToList();
        }

        public void AddClient<T>(T hub) where T : Hub
        {
            if (!TryClone(hub, out var clone)) return;

            var type = hub.GetType();
            var typeDict = Dictionary[type];

            typeDict.TryAdd(hub.Context.ConnectionId, clone);
        }

        public void RemoveClient<T>(T hub) where T : Hub
        {
            if (!CanClone(hub)) return;

            var type = hub.GetType();
            var typeDict = Dictionary[type];
            typeDict.TryRemove(hub.Context.ConnectionId, out var _);
        }

        /// <summary>
        /// return true only if hub has implement interface 'ICloneable'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hub"></param>
        /// <param name="clone"></param>
        /// <returns></returns>
        private bool TryClone<T>(T hub, out T clone) where T : Hub
        {
            clone = default;

            if (!CanClone(hub)) return false;

            clone = (hub as ICloneable).Clone() as T;
            return true;
        }

        private bool CanClone<T>(T hub) where T : Hub
        {
            return typeof(ICloneable).IsAssignableFrom(hub.GetType());
        }
    }
}
