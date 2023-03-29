using Microsoft.AspNetCore.Http;

namespace SignalR.SmartCore.Server.Filters.Authenticators
{
    /// <summary>
    /// Custom authentication layer interface, performing identity checks on HttpContext content during connection establishment and maintenance.
    /// </summary>
    public interface ISmartHubAuthenticator
    {
        /// <summary>
        /// Executes when the user successfully establishes a connection.
        /// </summary>
        /// <param name="context"> Encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns> indicates if it's a valid user</returns>
        public bool OnConnected(HttpContext context);

        /// <summary>
        /// Executes when the user sends a valid message. 
        /// This method will not execute when the user sends an unimplemented message method.
        /// </summary>
        /// <param name="context"> Encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns> indicates if it's a valid user</returns>
        public bool OnMethodInvoked(HttpContext context);
    }
}
