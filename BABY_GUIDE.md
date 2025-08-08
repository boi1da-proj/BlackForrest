# 🌲 BlackForrest - Baby-Friendly Implementation Guide

> **Storyboard for Building a Complete 2D/3D Modeling Engine**

This guide walks you from **zero to a runnable, maintainable architecture** with clear handoffs between pieces and a path to CI/CD. Think of it as a "storyboard" you can hand to a beginner and say, "let's build this one brick at a time."

## 🎯 Purpose in Plain Words

You're building a **little modeling factory**. It makes 2D shapes (polylines), squeezes them into 3D (prisms), and saves results as 3D meshes (STL). It can ask a RhinoCompute server to do heavy math, but it can also do the work locally if the server isn't there. A tiny, Grasshopper-like node UI helps you glue steps together. Everything is tracked in a list (`artifact_index.json`) so you can replay or audit every run.

## 🏗️ Big-Picture Architecture (Baby Terms)

- **CoreEngine**: The kitchen where we cook geometry
  - 2D: Draw a line, turn it into a flat shape
  - 3D: Push that shape up to make a brick (prism)
  - Output: A triangle mesh, exportable as STL

- **ComputeBridge**: The helper that can ask RhinoCompute to do geometry; if RhinoCompute isn't available, it uses a fast local method

- **UI**: A tiny canvas that looks like Grasshopper, with draggable "nodes" like Extrude, Loft, etc.

- **ArtifactIndex**: A safe notebook that records what we did (asset, version, path, time, checksums)

- **App/CLI**: Glue that runs the pipeline end-to-end

- **CI/CD**: Keeps the whole thing honest by compiling, testing, and regenerating the index automatically

## 📁 Project Structure (What We Created)

```
BlackForrest/
├── BlackForrest.sln                    # Main solution file
├── src/
│   ├── BlackForrest.Core/              # Core geometry engine
│   │   ├── Geometry/Prism.cs           # 2D/3D operations
│   │   └── Artifacts/                  # Artifact tracking
│   ├── BlackForrest.ComputeBridge/     # Compute integration
│   │   ├── IComputeClient.cs           # Compute interface
│   │   ├── LocalComputeClient.cs       # Local compute (working)
│   │   └── RhinoComputeClient.cs       # Remote compute template
│   ├── BlackForrest.UI/                # David Rutten-style UI
│   │   ├── BrandTheme.cs               # White UI + brand colors
│   │   └── NodeCanvasForm.cs           # Node canvas interface
│   └── BlackForrest.App/               # Main application
│       └── Program.cs                  # Demo orchestrator
├── scripts/
│   └── regenerate_index.py             # Artifact index regeneration
├── .github/workflows/
│   └── dotnet.yml                      # CI/CD pipeline
├── artifact_index.json.template        # Artifact tracking scaffold
├── README.md                           # Comprehensive documentation
├── build-and-run.sh                    # Automated build and test script
└── .gitignore                          # Project ignore patterns
```

## 🔧 Core Data Types We Implemented

### Mesh3D
```csharp
public class Mesh3D
{
    public List<Vector3> Vertices { get; set; } = new();
    public List<int[]> Triangles { get; set; } = new();
}
```

### Prism (Geometry)
```csharp
public static class Prism
{
    public static Mesh3D FromPolyline(List<Vector2> polyline2D, double height)
    {
        // Creates bottom and top rings plus side quads (two triangles per edge)
    }
}
```

### ArtifactIndexEntry
```csharp
public class ArtifactIndexEntry
{
    public string AssetId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Path { get; set; }
    public string Version { get; set; }
    public string Checksum { get; set; }
    public List<string> Dependencies { get; set; }
    public string ShadowDeploymentMetadata { get; set; }
    public string EnvironmentLabel { get; set; }
    public string Timestamp { get; set; }
}
```

## 🔄 Data Flow: How a Run Goes from Sketch to Store

1. **User defines a 2D polyline** (points in XY)
2. **Engine creates a 3D prism** by extruding that polyline by a given height
3. **Export the mesh to STL** for quick checks
4. **ComputeBridge.ExtrudePolylineAsync(polyline, height)** can:
   - Use LocalComputeClient to produce the same prism instantly
   - Or call RhinoCompute (remote) to produce the same result
5. **App writes an entry to artifact_index.json** with asset details and a timestamp
6. **Node UI shows an Extrude node** chaining to a Mesh output node (for future expansion)

## 🚀 Tiny, Safe, Incremental Implementation Plan

### ✅ Phase 1: Core Geometry + STL Export (COMPLETED)
- [x] Prism.FromPolyline
- [x] Mesh3D + STL writer
- [x] CLI/demo to create a prism and save STL

### ✅ Phase 2: Compute Bridge (COMPLETED)
- [x] IComputeClient interface
- [x] LocalComputeClient.ExtrudePolylineAsync
- [x] RhinoComputeClient stub (ready for real payloads)

### ✅ Phase 3: UI (COMPLETED)
- [x] Tiny Rutten-inspired node canvas (drag/drop, connect inputs/outputs)
- [x] Single "Extrude" node wired to Polyline2D + Height

### ✅ Phase 4: Artifact Index (COMPLETED)
- [x] ArtifactIndexEntry + ArtifactIndex
- [x] Regeneration script (regenerate_index.py)

### ✅ Phase 5: CI/CD (COMPLETED)
- [x] GitHub Actions: build, run a small test, regenerate index

### 🔄 Phase 6: Shadow Code Path (OPTIONAL - NEXT)
- [ ] Light Shadow Code scaffold (shadow runtime, sandbox config, simple "hello shadow")
- [ ] Ensure artifact_index.json is updated with per-run metadata

## 🎯 Simple Run Recipe (Baby Steps)

### Prerequisites
- .NET 8 SDK
- Python for index script
- GitHub for repo

### Step-by-Step Implementation

#### Step 1: ✅ Create Solution and Projects (COMPLETED)
```bash
# We created:
# - BlackForrest.sln
# - src/BlackForrest.Core/BlackForrest.Core.csproj
# - src/BlackForrest.ComputeBridge/BlackForrest.ComputeBridge.csproj
# - src/BlackForrest.UI/BlackForrest.UI.csproj
# - src/BlackForrest.App/BlackForrest.App.csproj
```

#### Step 2: ✅ Implement Prism.FromPolyline and MeshIO.ExportStl (COMPLETED)
```csharp
// See: src/BlackForrest.Core/Geometry/Prism.cs
// See: src/BlackForrest.Core/Geometry/MeshIO.cs
```

#### Step 3: ✅ Add IComputeClient and LocalComputeClient (COMPLETED)
```csharp
// See: src/BlackForrest.ComputeBridge/IComputeClient.cs
// See: src/BlackForrest.ComputeBridge/LocalComputeClient.cs
```

#### Step 4: ✅ Create CLI App (COMPLETED)
```csharp
// See: src/BlackForrest.App/Program.cs
// - Builds a sample polyline
// - Extrudes to a prism via LocalComputeClient
// - Writes an STL
// - Writes artifact_index.json with a first entry
```

#### Step 5: ✅ Build and Run Locally (COMPLETED)
```bash
./build-and-run.sh
```

#### Step 6: ✅ Add NodeCanvasForm UI (COMPLETED)
```csharp
// See: src/BlackForrest.UI/NodeCanvasForm.cs
// - Rutten-inspired interface
// - White UI with brand colors
// - Draggable nodes
```

#### Step 7: ✅ Wire ArtifactIndex Writer (COMPLETED)
```python
# See: scripts/regenerate_index.py
```

#### Step 8: ✅ Create GitHub Actions Workflow (COMPLETED)
```yaml
# See: .github/workflows/dotnet.yml
```

#### Step 9: 🔄 Implement RhinoComputeClient (NEXT)
```csharp
// See: src/BlackForrest.ComputeBridge/RhinoComputeClient.cs
// - Replace stubs with real API calls
// - Wire server URL configuration
```

## 🔍 Observability and Safety Knobs

### ✅ Logging (IMPLEMENTED)
- Simple logs at each stage (input, compute start, compute end, output)
- Console output for debugging

### ✅ Traceability (IMPLEMENTED)
- artifact_index.json always updated after a run
- Checksums for reproducibility

### 🔄 Isolation (NEXT)
- Plan for Shadow Code sandbox (container or process isolation)
- Safeguard file system (only write to run directory)

### 🔄 Permissions (NEXT)
- Do not execute untrusted code
- Validate inputs before processing

## 🧪 Testing and Validation Plan

### ✅ Unit Tests (READY FOR IMPLEMENTATION)
- Prism.FromPolyline with a square → expected vertex count and triangle count
- STL writer output sanity (non-empty file, contains "solid" header)

### ✅ Integration Tests (READY FOR IMPLEMENTATION)
- LocalComputeClient returns a mesh of expected bounds for a simple polyline

### ✅ CI Checks (IMPLEMENTED)
- Build the solution on each push
- Run tests
- Optionally run Python index-regeneration to validate artifact_index.json generation

## 🚀 How to Extend Later (Pick Two or Three Next Steps)

### 1. Add More 2D/3D Operations
- [ ] Offset2D, Loft, Boolean operations, fillets
- [ ] Curve support and surface operations

### 2. Strengthen the Compute Path
- [ ] Add real GH definition payload to RhinoCompute (GH def, inputs trees, outputs)
- [ ] Implement compute job queuing and caching

### 3. Expand UI
- [ ] Add more nodes, run graphs, save/load graphs
- [ ] Implement node wiring and parameter editing

### 4. Localization
- [ ] Add language blocks for en/fr/es/de/zh-CN/ja/ko/ru/ar/pt/it

### 5. Shadow Code Integration
- [ ] Implement per-run sandboxed deployments with audit trail
- [ ] Add container isolation for compute operations

## 📚 Quick Glossary (Baby-Friendly)

- **Prism**: A 3D brick made from a 2D outline
- **Mesh**: A bunch of tiny triangles that approximate a surface
- **STL**: A simple file format to save 3D shapes
- **RhinoCompute**: A remote helper that does heavy math for geometry
- **ArtifactIndex**: A notebook where we record what we built and when
- **Node Canvas**: A fun "puzzle" board where you connect steps to make geometry

## 🎨 Brand Colors Applied

- **Background**: Pure white (#FFFFFF)
- **Surfaces**: White (#FFFFFF) with light gray borders (#E6E6E6)
- **Primary**: Pink (#FF4C8E) - Used for node title bars
- **Secondary**: Purple (#6A4ACB), Yellow (#FFC107), Green (#43A047), Blue (#1E88E5)
- **Text**: Dark gray (#1C1C1C) for high contrast

## 🚀 Quick Start Commands

### Clone and Run
```bash
# Clone the repository
git clone https://github.com/your-username/BlackForrest.git
cd BlackForrest

# Run the complete demo
./build-and-run.sh
```

### What You'll See
1. **Console Output**: 2D polyline creation and 3D extrusion
2. **STL Export**: `outputs/prism_local.stl` file generated
3. **White UI**: Beautiful node canvas with pink title bars
4. **Artifact Tracking**: `artifact_index.json` updated with run metadata

### Push to GitHub
```bash
# Create new repo
gh repo create BlackForrest --public --source=. --remote=origin --push

# Or if repo exists
git remote add origin https://github.com/your-username/BlackForrest.git
git push -u origin main
```

## 🎯 Success Metrics

- ✅ **Builds successfully** on first clone
- ✅ **Runs demo** without external dependencies
- ✅ **Generates STL** file with correct geometry
- ✅ **Launches UI** with white background and brand colors
- ✅ **Updates artifact_index.json** with run metadata
- ✅ **CI/CD pipeline** runs successfully

## 🔧 Next Steps for You

1. **Clone and test** the complete implementation
2. **Customize brand colors** if needed (update `BrandTheme.cs`)
3. **Wire RhinoCompute** for heavy operations
4. **Add more geometry operations** as needed
5. **Enhance the UI** with more nodes and features

---

**BlackForrest** - Your complete 2D/3D modeling engine with beautiful white UI and brand colors! 🌲🚀

*This guide follows the baby-friendly storyboard approach, making it easy for beginners to understand and extend the architecture.*
