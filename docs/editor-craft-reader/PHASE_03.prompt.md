Read `docs/editor-craft-reader/PHASE_03.md` for full context before starting.

## Task

You are implementing Phase 3 of the Editor Craft Reader feature: MCP plumbing and the `get_current_craft` tool. Phase 2 (EditorService + kRPC procedure) must already be merged into `feature/editor-craft-reader`.

Work in branch `feature/editor-craft-reader-phase-3` (branch off `feature/editor-craft-reader`).

---

## 1. `src/MunControlProtocol.MCP/Krpc/IKrpcConnection.cs`

Read the file. Add one line to the interface:

```csharp
string GetCurrentCraft();
```

Place it after `GetKerbals()`, before the closing brace.

---

## 2. `src/MunControlProtocol.MCP/Krpc/MunControlProtocolStubs.cs`

Read the file. Add a new stub method to the `Service` class, following the exact pattern of `GetKerbals()` (no parameters, returns string):

```csharp
[global::KRPC.Client.Attributes.RPCAttribute ("MunControlProtocol", "GetCurrentCraft")]
public string GetCurrentCraft ()
{
    ByteString _data = connection.Invoke ("MunControlProtocol", "GetCurrentCraft");
    return (string)global::KRPC.Client.Encoder.Decode (_data, typeof(string), connection);
}
```

---

## 3. `src/MunControlProtocol.MCP/Krpc/KrpcConnection.cs`

Read the file. Add the implementation of the new interface member. Place it after `GetKerbals`:

```csharp
string IKrpcConnection.GetCurrentCraft() => MunControlProtocol.GetCurrentCraft();
```

---

## 4. New file: `src/MunControlProtocol.MCP/Tools/EditorTools.cs`

Read `src/MunControlProtocol.MCP/Tools/VesselsTools.cs` as the style reference.

```csharp
using System.Text.Json;
using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.Shared.Models;
using ModelContextProtocol.Server;

namespace MunControlProtocol.MCP.Tools;

[McpServerToolType]
internal sealed class EditorTools(IKrpcConnection connection)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns the craft currently open in the Vehicle Assembly Building (VAB) or Space Plane Hangar (SPH),
    /// or null if the editor is not open or no craft is loaded.
    /// Each part includes: name, title, dry mass (massT), resource mass (resourceMassT), cost,
    /// stageIndex (0 = last stage to fire), resources with current fill levels, and any installed
    /// modules (engine, tank, command pod, antenna, solar panel).
    /// Use stageIndex to group parts by stage, then apply the Tsiolkovsky formula (formula_delta_v)
    /// with stage wet/dry mass and mass-weighted ISP to estimate delta-v per stage.
    /// </summary>
    [McpServerTool(Name = "get_current_craft")]
    public Task<CraftDesign?> GetCurrentCraftAsync()
    {
        var json = connection.GetCurrentCraft();
        if (json == "null" || string.IsNullOrEmpty(json))
            return Task.FromResult<CraftDesign?>(null);

        var craft = JsonSerializer.Deserialize<CraftDesign>(json, JsonOptions);
        return Task.FromResult(craft);
    }
}
```

The `CraftDesign` type was added in Phase 1 to `MunControlProtocol.Shared.Models`.

---

## Build + Test Verification

```
dotnet build src/MunControlProtocol.MCP
dotnet test tests/MunControlProtocol.MCP.Tests
```

Both must succeed with zero errors/failures.

---

## Completion

Update `docs/editor-craft-reader/PROGRESS.md`: set Phase 3 status to `complete`, fill in the completed date.

Open a PR from `feature/editor-craft-reader-phase-3` targeting `feature/editor-craft-reader`.
