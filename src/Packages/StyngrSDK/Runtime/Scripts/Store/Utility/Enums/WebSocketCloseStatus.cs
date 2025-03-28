namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums
{
    /// <summary>
    /// Represents the web socket connection closure status.
    /// </summary>
    public enum WebSocketCloseStatus : ushort
    {
        /// <summary>
        /// 1000 indicates a normal closure, meaning that the purpose for
        /// which the connection was established has been fulfilled.
        /// </summary>
        NormalClosure = 1000,

        /// <summary>
        /// 1001 indicates that an endpoint is "going away", such as a server
        /// going down or a browser having navigated away from a page.
        /// </summary>
        ServerDown = 1001,

        /// <summary>
        /// 1002 indicates that an endpoint is terminating the connection due
        /// to a protocol error.
        /// </summary>
        ProtocolError = 1002,

        /// <summary>
        /// 1003 indicates that an endpoint is terminating the connection
        /// because it has received a type of data it cannot accept (e.g., an
        /// endpoint that understands only text data MAY send this if it 
        /// receives a binary message).
        /// </summary>
        InvalidData = 1003,

        /*
           Reserved.  The specific meaning might be defined in the future.
        Reserved: 1004,
        */

        /// <summary>
        /// 1005 is a reserved value and MUST NOT be set as a status code in a
        /// Close control frame by an endpoint. It is designated for use in
        /// applications expecting a status code to indicate that no status
        /// code was actually present.
        /// </summary>
        NoStatusCode = 1005,

        /// <summary>
        /// 1006 is a reserved value and MUST NOT be set as a status code in a
        /// Close control frame by an endpoint.  It is designated for use in
        /// applications expecting a status code to indicate that the
        /// connection was closed abnormally, e.g., without sending or
        /// receiving a Close control frame.
        /// </summary>
        ClosedAbnormally = 1006
    }
}
