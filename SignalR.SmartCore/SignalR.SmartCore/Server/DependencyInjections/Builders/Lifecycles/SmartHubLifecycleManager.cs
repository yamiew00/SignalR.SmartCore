namespace SignalR.SmartCore.Server.DependencyInjections.Builders.Lifecycles
{
    public class SmartHubLifecycleManager
    {
        public List<Registration> Registrations { get; } = new List<Registration>();

        internal void AddSingleton(Type serviceType,
                                   Type implementationType)
        {
            Registration registration = new Registration
            {
                ServiceType = serviceType,
                ImplmententType = implementationType,
                Lifecycle = SmartHubLifecycle.Singleton
            };
            Registrations.Add(registration);
        }

        internal void AddSingleton(Type serviceType,
                                   object instance)
        {
            Registration registration = new Registration
            {
                ServiceType = serviceType,
                Instance = instance,
                Lifecycle = SmartHubLifecycle.Singleton
            };
            Registrations.Add(registration);
        }

        internal void AddScoped(Type serviceType,
                                Type implementationType)
        {
            Registration registration = new Registration
            {
                ServiceType = serviceType,
                ImplmententType = implementationType,
                Lifecycle = SmartHubLifecycle.Scoped
            };
            Registrations.Add(registration);
        }

        internal void AddTransient(Type serviceType,
                                   Type implementationType)
        {
            Registration registration = new Registration
            {
                ServiceType = serviceType,
                ImplmententType = implementationType,
                Lifecycle = SmartHubLifecycle.Transient
            };
            Registrations.Add(registration);
        }
    }
}
