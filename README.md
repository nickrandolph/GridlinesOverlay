# GridlinesOverlay

A semi-transparent overlay control for Uno Platform and WinUI that displays gridlines for alignment purposes.

## Features

- 📏 Equal row and column spacing
- 🎨 Configurable color (default: Red)
- 🖌️ Configurable dash pattern (default: solid line)
- 🔍 Configurable opacity (default: 50%)
- ⌨️ **Ctrl+G**: Toggle visibility
- ⌨️ **G**: Cycle through spacing levels (10px to 100px in 10px increments)
- 🔝 Always on top but non-interactive (won't block user interactions)

## Installation

Install the package from NuGet:

```bash
dotnet add package GridlinesOverlay.Controls
```

## Usage

Add the control to your XAML page:

```xaml
<Page xmlns:controls="using:GridlinesOverlay.Controls">
  <Grid>
    <!-- Your content here -->
    
    <!-- GridlinesOverlay - Always on top but non-interactive -->
    <controls:GridlinesOverlay />
  </Grid>
</Page>
```

### Customization

You can customize the appearance of the gridlines:

```xaml
<controls:GridlinesOverlay 
    GridlineColor="Blue"
    GridlineOpacity="0.3"
    GridSpacing="20" />
```

For dashed lines:

```xaml
<controls:GridlinesOverlay>
  <controls:GridlinesOverlay.GridlineStrokeDashArray>
    <DoubleCollection>2,2</DoubleCollection>
  </controls:GridlinesOverlay.GridlineStrokeDashArray>
</controls:GridlinesOverlay>
```

### Keyboard Shortcuts

- **Ctrl+G**: Toggle gridlines visibility
- **G** (when gridlines are visible): Increase spacing by 10px
  - Cycles from 10px → 20px → 30px → ... → 100px → hidden → 10px → ...

## Building from Source

### Prerequisites

- .NET 10 SDK
- Uno Platform workloads

### Build

```bash
# Install workloads
dotnet workload install android

# Restore dependencies
dotnet restore

# Build the library
dotnet build src/GridlinesOverlay.Controls/GridlinesOverlay.Controls.csproj -c Release

# Build the sample app
dotnet build samples/GridlinesOverlay.Sample/GridlinesOverlay.Sample/GridlinesOverlay.Sample.csproj -c Release -f net10.0-desktop
```

### Run the Sample App

```bash
cd samples/GridlinesOverlay.Sample
dotnet run --project GridlinesOverlay.Sample/GridlinesOverlay.Sample.csproj -f net10.0-desktop
```

## License

MIT
