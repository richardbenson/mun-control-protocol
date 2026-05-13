using System;
using System.Collections.Generic;

namespace MunControlProtocol.Shared.Models;

public sealed class Vessel
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Situation { get; set; } = "";
    public string Body { get; set; } = "";
    public IList<string> CrewNames { get; set; } = Array.Empty<string>();
    public double Apoapsis { get; set; }
    public double Periapsis { get; set; }
    public double Inclination { get; set; }
}
