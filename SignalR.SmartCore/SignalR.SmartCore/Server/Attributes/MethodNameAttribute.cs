using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server.Attributes
{
    /// <summary>
    /// Customizes the name of a hub method.
    /// </summary>
    public class MethodNameAttribute : HubMethodNameAttribute
    {
        public MethodNameAttribute(string methodName) : base(methodName)
        {
        }
    }
}
