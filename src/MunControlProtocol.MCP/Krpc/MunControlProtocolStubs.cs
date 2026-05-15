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

namespace KRPC.Client.Services.MunControlProtocol
{
    /// <summary>
    /// Extension methods for MunControlProtocol service.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create an instance of the MunControlProtocol service.
        /// </summary>
        public static global::KRPC.Client.Services.MunControlProtocol.Service MunControlProtocol (this global::KRPC.Client.IConnection connection)
        {
            return new global::KRPC.Client.Services.MunControlProtocol.Service (connection);
        }
    }

    /// <summary>
    /// MunControlProtocol service.
    /// </summary>
    public class Service
    {
        global::KRPC.Client.IConnection connection;

        internal Service (global::KRPC.Client.IConnection serverConnection)
        {
            connection = serverConnection;
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetBuildingLevels")]
        public string GetBuildingLevels ()
        {
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetBuildingLevels");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetCurrentCraft")]
        public string GetCurrentCraft ()
        {
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetCurrentCraft");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetDifficultySettings")]
        public string GetDifficultySettings ()
        {
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetDifficultySettings");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetKerbals")]
        public string GetKerbals ()
        {
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetKerbals");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetPartByName")]
        public string GetPartByName (string name)
        {
            var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (name, typeof(string))
            };
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetPartByName", _args);
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetPartsByCategory")]
        public string GetPartsByCategory (string category)
        {
            var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (category, typeof(string))
            };
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetPartsByCategory", _args);
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetSciencePerBodySummary")]
        public string GetSciencePerBodySummary ()
        {
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetSciencePerBodySummary");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetScienceSubjects")]
        public string GetScienceSubjects (string body, string situation)
        {
            var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (body, typeof(string)),
                global::KRPC.Client.Encoder.Encode (situation, typeof(string))
            };
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetScienceSubjects", _args);
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetTechTree")]
        public string GetTechTree ()
        {
            ByteString _data = connection.Invoke ("MunControlProtocol", "GetTechTree");
            return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
        }
    }
}
