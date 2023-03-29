namespace SignalR.SmartCore.Server.DependencyInjections
{
    internal static class SmartHubTypeExtensions
    {
        internal static bool IsConcreteAndSubClassOfSmartHub(this Type type)
        {
            bool isConcrete = !type.IsAbstract && !type.IsInterface;
            bool isDerivedFromSmartHub = false;

            while (type.BaseType != null)
            {
                if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(SmartHub<>))
                {
                    isDerivedFromSmartHub = true;
                    break;
                }
                type = type.BaseType;
            }

            return isConcrete && isDerivedFromSmartHub;
        }

        /// <summary>
        /// For every SmartHub<GenericType>, return GenericType
        /// </summary>
        /// <param name="smartHubType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static Type GetSmartHubGenericTypeParameter(this Type smartHubType)
         { 
            Type baseType = smartHubType.BaseType;

            while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(SmartHub<>)))
            {
                baseType = baseType.BaseType;
            }

            //
            if (baseType == null || !baseType.IsGenericType) throw new Exception($"{smartHubType.Name} is is not a derived class of {typeof(SmartHub<>).Name}");

            Type[] genericArguments = baseType.GetGenericArguments();

            if (genericArguments.Length == 1 && !genericArguments[0].IsValueType)
            {
                return genericArguments[0];
            }
            else
            {
                throw new Exception($"{smartHubType.Name} does not match the conditions.");
            }
        }
    }
}
