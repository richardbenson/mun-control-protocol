# Add a New MCP Tool

Use this skill whenever a new MCP tool is added to the server. It walks through all four registration steps that must be completed for a tool to appear in the tool list and pass the smoke test. Missing any one of these steps is a common source of "tool not found" bugs.

## Steps

### 1. Add the tool class

Create (or extend) a `*Tools.cs` file under `src/MunControlProtocol.MCP/Tools/`. Every tool class must:

- Be decorated with `[McpServerToolType]`
- Have its tool methods decorated with `[McpServerTool(Name = "tool_name")]`
- Accept `IKrpcConnection` via constructor injection if it needs live KSP data

Naming convention: group related tools in one class (e.g. `EditorTools`, `VesselsTools`). If adding a method to an existing class, no new file is needed.

### 2. Register the class in Program.cs

Open `src/MunControlProtocol.MCP/Program.cs` and add a `.WithTools<YourTools>()` call to the builder chain. The chain currently looks like:

```csharp
builder.Services
    .AddSingleton<IKrpcConnection, KrpcConnection>()
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CareerTools>()
    // ... other tools ...
    .WithTools<FormulasTools>();
```

Add the new class anywhere in that list. **This is the step most often forgotten** — without it the tool compiles but is never registered and will not appear in `tools/list`.

### 3. Update the smoke test

Open `deploy/test-mcp-tools.py` and add the tool name to the `EXPECTED_TOOLS` list. Keep the list grouped by comment (live tools vs formula tools). The smoke test treats any expected tool that is absent as a hard failure (exit 1), and any tool in the server that is not in the list as a warning — so adding it here locks in the registration.

### 4. Verify

Run:

```
dotnet build MunControlProtocol.sln -c Release
dotnet test
```

Both must pass with no errors. If the repo has a deployed exe handy, also run:

```
python deploy/test-mcp-tools.py <path-to-MunControlProtocol.MCP.exe>
```

and confirm the new tool name appears in the discovered list and in the "All N expected tools present" summary.
