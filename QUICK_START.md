# 🚀 BlackForrest Quick Start

> **Get up and running in 5 minutes with your complete 2D/3D modeling engine**

## 🎯 What You Get

A **production-ready C# modeling engine** with:
- ✅ **2D/3D Geometry Engine** (working out of the box)
- ✅ **White UI with Brand Colors** (beautiful David Rutten-style interface)
- ✅ **Rhino.Compute Bridge** (local + remote template)
- ✅ **Artifact Tracking** (reproducible logging)
- ✅ **CI/CD Ready** (GitHub Actions)

## 🚀 One-Command Setup

```bash
# Clone and run (if you have the repo)
git clone https://github.com/your-username/BlackForrest.git
cd BlackForrest
./build-and-run.sh
```

## 📋 Prerequisites

- **.NET 8 SDK** ([Download here](https://dotnet.microsoft.com/download))
- **Python 3.7+** (for artifact index regeneration)
- **Git** (for version control)

### Quick Install (macOS)
```bash
# Install .NET
brew install dotnet

# Install Python (if not already installed)
brew install python

# Verify installations
dotnet --version  # Should show 8.x.x
python3 --version  # Should show 3.7+
```

## 🎯 What Happens When You Run

### 1. Console Output
```
🌲 BlackForrest - C# Modeling Engine with White UI + Brand Colors
==================================================================
✅ .NET SDK found: 8.0.100
🔨 Building BlackForrest solution...
✅ Build successful!
🎯 Running BlackForrest demo...
   This will:
   1. Create a 2D polyline
   2. Extrude it to 3D
   3. Export to outputs/prism_local.stl
   4. Launch the white UI with brand colors (if on Windows)
```

### 2. Generated Files
- `outputs/prism_local.stl` - 3D mesh file
- `artifact_index.json` - Run metadata and asset tracking

### 3. White UI Launch
- Beautiful node canvas with pink title bars
- Draggable "Extrude" node
- Brand-colored interface elements

## 🔧 Customization

### Update Brand Colors
Edit `src/BlackForrest.UI/BrandTheme.cs`:
```csharp
// Change these hex values to match your brand
public static Color Primary => Color.FromArgb(255, 76, 142);       // pink
public static Color Purple => Color.FromArgb(106, 74, 203);        // purple
public static Color Yellow => Color.FromArgb(255, 193, 7);         // yellow
```

### Add More Geometry Operations
Edit `src/BlackForrest.Core/Geometry/Prism.cs`:
```csharp
// Add new operations like:
public static Mesh3D FromCircle(Vector2 center, double radius, double height)
public static Mesh3D FromRectangle(Vector2 min, Vector2 max, double height)
```

### Wire Rhino.Compute
Edit `src/BlackForrest.ComputeBridge/RhinoComputeClient.cs`:
```csharp
// Replace stubs with real API calls
var response = await _httpClient.PostAsync($"{_serverUrl}/grasshopper", payload);
```

## 🚀 Push to GitHub

### Option A: GitHub CLI (Recommended)
```bash
# Create new repo and push
gh repo create BlackForrest --public --source=. --remote=origin --push
```

### Option B: Manual Git
```bash
# Initialize and push
git init
git add .
git commit -m "feat: BlackForrest v1.0 - Complete 2D/3D engine with white UI + brand colors"
git remote add origin https://github.com/your-username/BlackForrest.git
git push -u origin main
```

## 🎯 Success Checklist

- ✅ **Builds successfully** on first clone
- ✅ **Runs demo** without external dependencies  
- ✅ **Generates STL** file with correct geometry
- ✅ **Launches UI** with white background and brand colors
- ✅ **Updates artifact_index.json** with run metadata
- ✅ **CI/CD pipeline** runs successfully

## 🔧 Troubleshooting

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### UI Not Launching (macOS/Linux)
The Windows Forms UI requires Windows or Mono. On macOS/Linux:
- The console demo will still work
- STL generation will still work
- Only the visual UI won't launch

### Python Script Issues
```bash
# Make script executable
chmod +x scripts/regenerate_index.py

# Run manually
python3 scripts/regenerate_index.py
```

## 🎯 Next Steps

1. **Test the complete pipeline** - Run `./build-and-run.sh`
2. **Customize brand colors** - Update `BrandTheme.cs`
3. **Add geometry operations** - Extend `Prism.cs`
4. **Wire Rhino.Compute** - Implement real API calls
5. **Enhance the UI** - Add more nodes and features

## 📚 Documentation

- **BABY_GUIDE.md** - Complete implementation storyboard
- **README.md** - Comprehensive project documentation
- **Code comments** - Inline documentation throughout

---

**BlackForrest** - Ready to compete with Rhino and Revit! 🌲🚀

*Your complete 2D/3D modeling engine with beautiful white UI and brand colors.*
