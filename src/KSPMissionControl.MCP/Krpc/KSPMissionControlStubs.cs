using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#if NET35
using systemAlias = global::KRPC.Client.Compatibility;
using genericCollectionsAlias = global::KRPC.Client.Compatibility;
#else
using systemAlias = global::System;
using genericCollectionsAlias = global::System.Collections.Generic;
#endif

namespace KRPC.Client.Services.KSPMissionControl
{
    /// <summary>
    /// Extension methods for KSPMissionControl service.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create an instance of the KSPMissionControl service.
        /// </summary>
        public static global::KRPC.Client.Services.KSPMissionControl.Service KSPMissionControl (this global::KRPC.Client.IConnection connection)
        {
            return new global::KRPC.Client.Services.KSPMissionControl.Service (connection);
        }
    }

    /// <summary>
    /// KSPMissionControl service.
    /// </summary>
    public class Service
    {
        global::KRPC.Client.IConnection connection;

        internal Service (global::KRPC.Client.IConnection serverConnection)
        {
            connection = serverConnection;
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("KSPMissionControl", "GetPartByName")]
        public string GetPartByName (string name)
        {
            var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (name, typeof(string))
            };
            ByteString _data = connection.Invoke ("KSPMissionControl", "GetPartByName", _args);
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("KSPMissionControl", "GetPartsByCategory")]
        public string GetPartsByCategory (string category)
        {
            var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (category, typeof(string))
            };
            ByteString _data = connection.Invoke ("KSPMissionControl", "GetPartsByCategory", _args);
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("KSPMissionControl", "GetTechTree")]
        public string GetTechTree ()
        {
            ByteString _data = connection.Invoke ("KSPMissionControl", "GetTechTree");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }
    }
}
