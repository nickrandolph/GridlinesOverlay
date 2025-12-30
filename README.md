# GridlinesOverlay

A semi-transparent overlay control for Uno Platform and WinUI that displays gridlines for alignment purposes.

## Features

- 📏 Equal row and column spacing
- 🎨 Configurable color (default: Red)
- 🖌️ Configurable dash pattern (default: solid line)
- 🔍 Configurable opacity (default: 50%)
- ⌨️ **Ctrl+G**: Show/cycle spacing levels (8px to 64px in 8px increments)
- ⌨️ **G**: Hide gridlines
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

You can also customize the spacing behavior when cycling with Ctrl+G:

```xaml
<controls:GridlinesOverlay 
    DefaultSpacing="10"
    MinSpacing="5"
    MaxSpacing="100"
    SpacingIncrement="5" />
```

**Spacing Properties:**
- `DefaultSpacing`: The default spacing used when gridlines are made visible (default: 8)
- `MinSpacing`: The minimum spacing used when cycling. If null, DefaultSpacing is used (default: null)
- `MaxSpacing`: The maximum spacing when cycling (default: 64)
- `SpacingIncrement`: The increment used when cycling through spacing values (default: 8)

### Keyboard Shortcuts

- **Ctrl+G** (when hidden): Show gridlines with default spacing (8px)
- **Ctrl+G** (when visible, repeatedly with Ctrl held): Cycle spacing from 8px → 16px → 24px → ... → 64px → 8px → ...
- **G** (when visible): Hide gridlines

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
