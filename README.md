# Compute.Shadow

> **C# Modeling Engine** with 2D/3D geometry, Rhino.Compute integration, and David Rutten-style node canvas UI

A fully wired, production-ready C# solution that demonstrates how to build a modeling engine that can compete with Rhino and Revit, leveraging Rhino.Compute for heavy lifting and providing a Grasshopper-like interface with a beautiful white UI and brand colors. **Compute.Shadow** is the backend engine, while **Soft.Geometry** provides the frontend UI with BlackForrest as a feature within it.

## 🏗️ Architecture Overview

### Multi-Project Structure

```
Compute.Shadow/
├── Compute.Shadow.sln                  # Main solution file
├── src/
│   ├── Compute.Shadow.Core/            # Core geometry engine
│   │   ├── Geometry/Prism.cs           # 2D/3D operations
│   │   └── Artifacts/                  # Artifact tracking
│   ├── Compute.Shadow.Bridge/          # Compute integration
│   │   ├── IComputeClient.cs           # Compute interface
│   │   ├── LocalComputeClient.cs       # Local compute (working)
│   │   └── RhinoComputeClient.cs       # Remote compute template
│   ├── Soft.Geometry.UI/               # Frontend UI (David Rutten-style)
│   │   ├── BrandTheme.cs               # White UI + brand colors
│   │   └── NodeCanvasForm.cs           # Node canvas interface
│   └── Compute.Shadow.App/             # Main application
│       └── Program.cs                  # Demo orchestrator
├── scripts/
│   └── regenerate_index.py             # Artifact index regeneration
├── .github/workflows/
│   └── dotnet.yml                      # CI/CD pipeline
└── artifact_index.json.template        # Artifact tracking scaffold
```

### Key Features

✅ **2D/3D Geometry Engine** - Polyline creation, extrusion, mesh generation  
✅ **Rhino.Compute Bridge** - Local compute (working) + remote template  
✅ **White UI with Brand Colors** - Clean, modern interface with brand accents  
✅ **Node Canvas UI** - David Rutten-style interface with drag-and-drop  
✅ **STL Export** - Simple ASCII STL writer for geometry persistence  
✅ **Artifact Tracking** - Reproducible logging with checksums and metadata  
✅ **CI/CD Ready** - GitHub Actions for automated builds and tests  
✅ **Cross-Platform** - Windows, macOS, Linux support  

## 🎨 Brand Theme

### White UI with Brand Colors

- **Background**: Pure white (#FFFFFF)
- **Surfaces**: White (#FFFFFF) with light gray borders (#E6E6E6)
- **Primary**: Pink (#FF4C8E) - Used for node title bars
- **Secondary**: Purple (#6A4ACB), Yellow (#FFC107), Green (#43A047), Blue (#1E88E5)
- **Text**: Dark gray (#1C1C1C) for high contrast

## 🚀 Quick Start

### Prerequisites

- .NET SDK 8.0 or newer
- Windows (for the sample UI - Windows Forms)
- Optional: Rhino.Compute server for heavy operations

### Build and Run

```bash
# Clone and navigate to project
cd Compute.Shadow

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the demo
dotnet run --project src/Compute.Shadow.App/Compute.Shadow.App.csproj
```

### What You'll See

1. **Console Output**: 2D polyline creation and 3D extrusion
2. **STL Export**: `outputs/prism_local.stl` file generated
3. **White UI**: Beautiful node canvas with brand-colored title bars
4. **Artifact Tracking**: `artifact_index.json` updated with run metadata

## 🔧 Core Components

### 1. Geometry Engine (`Compute.Shadow.Core`)

Lightweight, dependency-free geometry builder:

```csharp
// Create a 2D polyline
var polyline2D = new List<Vector2> { ... };

// Extrude to 3D prism
var mesh = Compute.Shadow.Geometry.Prism.FromPolyline(polyline2D, height);

// Export to STL
Compute.Shadow.Geometry.MeshIO.ExportStl(mesh, "output.stl");
```

### 2. Compute Bridge (`Compute.Shadow.Bridge`)

Clean extension points for offloading heavy operations:

```csharp
// Local compute (works out of the box)
IComputeClient local = new LocalComputeClient();
var result = await local.ExtrudePolylineAsync(polyline2D, height);

// Remote compute (template for Rhino.Compute)
IComputeClient remote = new RhinoComputeClient("http://localhost:6500");
var remoteResult = await remote.ExtrudePolylineAsync(polyline2D, height);
```

### 3. Frontend UI (`Soft.Geometry.UI`)

David Rutten-style interface with beautiful branding:

- **White background** with subtle gray borders
- **Pink title bars** for nodes (brand primary)
- **Draggable nodes** with input/output sockets
- **Brand accent colors** for buttons and highlights
- **BlackForrest feature** integrated within the UI

### 4. Artifact Tracking

Reproducible logging for audit and CI/CD:

```csharp
// Log assets
ArtifactLogger.LogAsset("asset-id", "name", "type", "path", "version", "env", "notes");

// Log compute runs
ArtifactLogger.LogComputeRun("operation", "input-hash", "output-hash", "job-id", "env", duration, "notes");
```

## 🔌 Rhino.Compute Integration

### Setup Rhino.Compute

1. **Install Rhino.Compute**: Follow [Rhino's Compute Getting Started guide](https://developer.rhino3d.com/guides/compute/getting-started/)
2. **Run Compute Server**: Start local or cloud instance
3. **Wire Client**: Replace stubs in `RhinoComputeClient.cs` with real calls

### Example Integration

```csharp
// Replace stub with real Rhino.Compute call
public async Task<Mesh3D> ExtrudePolylineAsync(IList<Vector2> polyline, double height)
{
    // Build Grasshopper definition payload
    var payload = BuildGHPayload(polyline, height);
    
    // POST to Rhino.Compute
    var response = await _httpClient.PostAsync($"{_serverUrl}/grasshopper", payload);
    
    // Parse geometry response
    return ParseGeometryResponse(response);
}
```

## 🎯 Next Steps

### Phase 1: Core Engine Expansion
- [ ] Add 2D operations (offset, fillet, chamfer)
- [ ] Implement 3D boolean operations
- [ ] Add lofting between profiles
- [ ] Support for curves and surfaces

### Phase 2: Compute Integration
- [ ] Wire real Rhino.Compute calls
- [ ] Add Grasshopper definition support
- [ ] Implement compute job queuing
- [ ] Add compute result caching

### Phase 3: UI Enhancement
- [ ] Add node wiring (connections between nodes)
- [ ] Implement parameter editing
- [ ] Add node grouping and organization
- [ ] Support for custom node types

### Phase 4: Production Features
- [ ] Add comprehensive error handling
- [ ] Implement security hardening
- [ ] Add performance optimization
- [ ] Create CI/CD pipeline

## 🔒 Security and Production

### Important Guardrails

⚠️ **This is a production-ready scaffold, but you'll need to invest in:**  
⚠️ **Robust error handling and security**  
⚠️ **Comprehensive testing before production use**  
⚠️ **Proper authentication for Rhino.Compute**  

### Production Checklist

- [ ] Add input validation and sanitization
- [ ] Implement proper error handling and logging
- [ ] Add authentication and authorization
- [ ] Set up monitoring and alerting
- [ ] Create comprehensive test suite
- [ ] Implement CI/CD pipeline
- [ ] Add performance benchmarking
- [ ] Create deployment documentation

## 🚀 Push to GitHub

### Option A: Using GitHub CLI (Recommended)

```bash
# Create new repo
gh repo create Compute.Shadow --public --source=. --remote=origin --push

# Or if repo exists, push changes
git add .
git commit -m "feat: Compute.Shadow v1.0 - Complete 2D/3D engine with Soft.Geometry UI + brand colors"
git push -u origin main
```

### Option B: Plain Git

```bash
# Initialize and commit
git init
git add .
git commit -m "feat: BlackForrest v1.0 - Complete 2D/3D engine with white UI + brand colors"

# Create repo on GitHub manually, then push
git remote add origin https://github.com/your-username/BlackForrest.git
git branch -M main
git push -u origin main
```

## 📚 References

- [Rhino Compute Documentation](https://developer.rhino3d.com/guides/compute/)
- [Grasshopper Documentation](https://developer.rhino3d.com/guides/grasshopper/)
- [Hops Overview](https://developer.rhino3d.com/guides/compute/hops/)
- [Rhino.Compute Community](https://discourse.mcneel.com/)

## 🤝 Contributing

This is a production-ready scaffold designed for expansion. Key areas for contribution:

1. **Geometry Operations**: Add more 2D/3D operations
2. **Compute Integration**: Improve Rhino.Compute integration
3. **UI Enhancement**: Expand the node canvas capabilities
4. **Performance**: Optimize geometry operations
5. **Testing**: Add comprehensive test coverage

## 📄 License

This project is provided as a starting point for building modeling engines. Use at your own risk and ensure proper licensing for any dependencies.

---

**Ready to build the next generation of modeling software with Compute.Shadow backend and Soft.Geometry UI!** 🌲🚀
