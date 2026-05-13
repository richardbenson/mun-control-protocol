// KRPC.Core stub — compile-time only, no runtime use.
using System;

namespace KRPC.Service
{
    public enum GameScene { All, Flight, SpaceCenter, TrackingStation, Editor }
}

namespace KRPC.Service.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class KRPCServiceAttribute : System.Attribute
    {
        public string Name { get; set; }
        public KRPC.Service.GameScene GameScene { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class KRPCProcedureAttribute : System.Attribute { }
}
