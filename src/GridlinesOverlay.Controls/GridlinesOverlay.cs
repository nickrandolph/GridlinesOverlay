using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.System;

namespace GridlinesOverlay.Controls;

/// <summary>
/// A semi-transparent overlay control that displays gridlines for alignment purposes.
/// </summary>
public class GridlinesOverlay : Canvas
{
    private SolidColorBrush? _cachedBrush;
    private readonly List<Line> _linePool = new List<Line>();
    private int _linePoolIndex = 0;
    private bool _isInSpacingCycleMode = false;

    /// <summary>
    /// Identifies the DefaultSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty DefaultSpacingProperty =
        DependencyProperty.Register(
            nameof(DefaultSpacing),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(8.0, OnDefaultSpacingChanged));

    /// <summary>
    /// Identifies the MinSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty MinSpacingProperty =
        DependencyProperty.Register(
            nameof(MinSpacing),
            typeof(double?),
            typeof(GridlinesOverlay),
            new PropertyMetadata(null, OnMinSpacingChanged));

    /// <summary>
    /// Identifies the MaxSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxSpacingProperty =
        DependencyProperty.Register(
            nameof(MaxSpacing),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(64.0, OnMaxSpacingChanged));

    /// <summary>
    /// Identifies the SpacingIncrement dependency property.
    /// </summary>
    public static readonly DependencyProperty SpacingIncrementProperty =
        DependencyProperty.Register(
            nameof(SpacingIncrement),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(8.0, OnSpacingIncrementChanged));

    /// <summary>
    /// Identifies the GridSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty GridSpacingProperty =
        DependencyProperty.Register(
            nameof(GridSpacing),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(8.0, OnGridSpacingChanged));

    /// <summary>
    /// Identifies the GridlineColor dependency property.
    /// </summary>
    public static readonly DependencyProperty GridlineColorProperty =
        DependencyProperty.Register(
            nameof(GridlineColor),
            typeof(Windows.UI.Color),
            typeof(GridlinesOverlay),
            new PropertyMetadata(Colors.Red, OnGridlineColorChanged));

    /// <summary>
    /// Identifies the GridlineOpacity dependency property.
    /// </summary>
    public static readonly DependencyProperty GridlineOpacityProperty =
        DependencyProperty.Register(
            nameof(GridlineOpacity),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(0.5, OnGridlineOpacityChanged));

    /// <summary>
    /// Identifies the GridlineStrokeDashArray dependency property.
    /// </summary>
    public static readonly DependencyProperty GridlineStrokeDashArrayProperty =
        DependencyProperty.Register(
            nameof(GridlineStrokeDashArray),
            typeof(DoubleCollection),
            typeof(GridlinesOverlay),
            new PropertyMetadata(null, OnGridlineStrokeDashArrayChanged));

    /// <summary>
    /// Identifies the IsShortcutKeyEnabled dependency property.
    /// </summary>
    public static readonly DependencyProperty IsShortcutKeyEnabledProperty =
        DependencyProperty.Register(
            nameof(IsShortcutKeyEnabled),
            typeof(bool),
            typeof(GridlinesOverlay),
            new PropertyMetadata(true, OnIsShortcutKeyEnabledChanged));

    /// <summary>
    /// Gets or sets the default spacing used when the gridlines are made visible.
    /// Must be a positive value.
    /// </summary>
    public double DefaultSpacing
    {
        get => (double)GetValue(DefaultSpacingProperty);
        set => SetValue(DefaultSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum spacing used when cycling through the gridline spacing.
    /// If null, the DefaultSpacing value is used.
    /// </summary>
    public double? MinSpacing
    {
        get => (double?)GetValue(MinSpacingProperty);
        set => SetValue(MinSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum spacing that can be used when cycling through the gridline spacing.
    /// Must be a positive value.
    /// </summary>
    public double MaxSpacing
    {
        get => (double)GetValue(MaxSpacingProperty);
        set => SetValue(MaxSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the increment used when cycling through the gridline spacing.
    /// Must be a positive value.
    /// </summary>
    public double SpacingIncrement
    {
        get => (double)GetValue(SpacingIncrementProperty);
        set => SetValue(SpacingIncrementProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between gridlines.
    /// Must be a positive value.
    /// </summary>
    public double GridSpacing
    {
        get => (double)GetValue(GridSpacingProperty);
        set
        {
            // Validate that spacing is positive
            if (value > 0)
            {
                SetValue(GridSpacingProperty, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of the gridlines.
    /// </summary>
    public Windows.UI.Color GridlineColor
    {
        get => (Windows.UI.Color)GetValue(GridlineColorProperty);
        set => SetValue(GridlineColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the opacity of the gridlines.
    /// Must be between 0.0 and 1.0.
    /// </summary>
    public double GridlineOpacity
    {
        get => (double)GetValue(GridlineOpacityProperty);
        set
        {
            // Clamp opacity between 0.0 and 1.0
            var clampedValue = Math.Max(0.0, Math.Min(1.0, value));
            SetValue(GridlineOpacityProperty, clampedValue);
        }
    }

    /// <summary>
    /// Gets or sets the stroke dash array for the gridlines.
    /// </summary>
    public DoubleCollection? GridlineStrokeDashArray
    {
        get => (DoubleCollection?)GetValue(GridlineStrokeDashArrayProperty);
        set => SetValue(GridlineStrokeDashArrayProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the keyboard shortcut is enabled.
    /// When false, the Ctrl+G keyboard shortcut will not respond.
    /// </summary>
    public bool IsShortcutKeyEnabled
    {
        get => (bool)GetValue(IsShortcutKeyEnabledProperty);
        set => SetValue(IsShortcutKeyEnabledProperty, value);
    }

    public GridlinesOverlay()
    {
        IsHitTestVisible = false;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Only attach keyboard handlers if the control is enabled
        if (IsShortcutKeyEnabled)
        {
            AttachKeyboardHandlers();
        }
        DrawGridlines();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Remove keyboard handler
        DetachKeyboardHandlers();

        // Reset spacing cycle mode so a new session starts from the initial state
        _isInSpacingCycleMode = false;
    }

    private static void OnIsShortcutKeyEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            if ((bool)e.NewValue)
            {
                // Control was enabled - attach keyboard handlers if loaded
                if (overlay.IsLoaded)
                {
                    overlay.AttachKeyboardHandlers();
                }
            }
            else
            {
                // Control was disabled - detach keyboard handlers
                overlay.DetachKeyboardHandlers();
            }
        }
    }

    private void AttachKeyboardHandlers()
    {
        // Find the root element to attach keyboard handler
        if (XamlRoot?.Content is UIElement rootElement)
        {
            // Ensure no duplicate subscriptions by removing existing handlers first
            rootElement.KeyDown -= OnRootKeyDown;
            rootElement.KeyUp -= OnRootKeyUp;
            rootElement.KeyDown += OnRootKeyDown;
            rootElement.KeyUp += OnRootKeyUp;
        }
    }

    private void DetachKeyboardHandlers()
    {
        // Remove keyboard handler
        if (XamlRoot?.Content is UIElement rootElement)
        {
            rootElement.KeyDown -= OnRootKeyDown;
            rootElement.KeyUp -= OnRootKeyUp;
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawGridlines();
    }

    private void OnRootKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Only handle keyboard shortcuts if the control is enabled
        if (!IsShortcutKeyEnabled)
        {
            return;
        }

        var isCtrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (isCtrlPressed && e.Key == VirtualKey.G)
        {
            if (!_isInSpacingCycleMode)
            {
                // First Ctrl+G press: toggle visibility
                if (Visibility == Visibility.Collapsed)
                {
                    // Show with default spacing
                    GridSpacing = DefaultSpacing;
                    Visibility = Visibility.Visible;
                }
                else
                {
                    // Hide gridlines
                    Visibility = Visibility.Collapsed;
                }
                // Enter spacing cycle mode for subsequent G presses
                _isInSpacingCycleMode = true;
            }
            else
            {
                // Subsequent G presses while Ctrl is held: cycle spacing
                // Only cycle if gridlines are visible
                if (Visibility == Visibility.Visible)
                {
                    var minSpacing = MinSpacing ?? DefaultSpacing;
                    var newSpacing = GridSpacing + SpacingIncrement;
                    // Once max is reached, reset to minimum and continue
                    GridSpacing = newSpacing > MaxSpacing ? minSpacing : newSpacing;
                }
            }
            e.Handled = true;
        }
    }

    private void OnRootKeyUp(object sender, KeyRoutedEventArgs e)
    {
        // Only handle keyboard shortcuts if the control is enabled
        if (!IsShortcutKeyEnabled)
        {
            return;
        }

        // Reset spacing cycle mode when Ctrl is released
        // Check for generic Control and specific Left/Right Control keys to handle all keyboard configurations
        if (e.Key == VirtualKey.Control || e.Key == VirtualKey.LeftControl || e.Key == VirtualKey.RightControl)
        {
            _isInSpacingCycleMode = false;
        }
    }

    private static void OnGridSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            // Additional validation in case value is set via binding
            var newValue = (double)e.NewValue;
            if (newValue <= 0)
            {
                // Determine a safe fallback spacing
                var fallback = overlay.DefaultSpacing;
                if (fallback <= 0)
                {
                    // Hardcoded safe fallback to avoid infinite recursion with invalid DefaultSpacing
                    fallback = 10.0;
                }

                // Avoid recursion by only setting if value is meaningfully different from fallback
                if (Math.Abs(fallback - newValue) > 0.0001)
                {
                    overlay.SetValue(GridSpacingProperty, fallback);
                }
                return;
            }
            overlay.DrawGridlines();
        }
    }

    private static void OnGridlineColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            overlay.InvalidateBrush();
            overlay.DrawGridlines();
        }
    }

    private static void OnGridlineOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            // Additional validation in case value is set via binding
            var newValue = (double)e.NewValue;
            if (newValue < 0.0 || newValue > 1.0)
            {
                var clampedValue = Math.Max(0.0, Math.Min(1.0, newValue));
                // Avoid recursion by only setting if value is different
                if (Math.Abs(clampedValue - newValue) > 0.0001)
                {
                    overlay.SetValue(GridlineOpacityProperty, clampedValue);
                    return;
                }
            }
            overlay.InvalidateBrush();
            overlay.DrawGridlines();
        }
    }

    private static void OnGridlineStrokeDashArrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            overlay.DrawGridlines();
        }
    }

    private static void OnDefaultSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            var newValue = (double)e.NewValue;
            if (double.IsNaN(newValue) || double.IsInfinity(newValue) || newValue <= 0.0)
            {
                // Revert to default value only if different to avoid recursion
                const double defaultValue = 8.0;
                if (Math.Abs(newValue - defaultValue) > 0.0001)
                {
                    overlay.SetValue(DefaultSpacingProperty, defaultValue);
                }
            }
        }
    }

    private static void OnMinSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            var newValue = e.NewValue as double?;
            if (newValue.HasValue)
            {
                if (double.IsNaN(newValue.Value) || double.IsInfinity(newValue.Value) || newValue.Value <= 0.0)
                {
                    // Revert to null (use DefaultSpacing) only if different to avoid recursion
                    overlay.SetValue(MinSpacingProperty, null);
                }
            }
        }
    }

    private static void OnMaxSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            var newValue = (double)e.NewValue;
            if (double.IsNaN(newValue) || double.IsInfinity(newValue) || newValue <= 0.0)
            {
                // Revert to default value only if different to avoid recursion
                const double defaultValue = 64.0;
                if (Math.Abs(newValue - defaultValue) > 0.0001)
                {
                    overlay.SetValue(MaxSpacingProperty, defaultValue);
                }
            }
        }
    }

    private static void OnSpacingIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            var newValue = (double)e.NewValue;
            if (double.IsNaN(newValue) || double.IsInfinity(newValue) || newValue <= 0.0)
            {
                // Revert to default value only if different to avoid recursion
                const double defaultValue = 8.0;
                if (Math.Abs(newValue - defaultValue) > 0.0001)
                {
                    overlay.SetValue(SpacingIncrementProperty, defaultValue);
                }
            }
        }
    }

    private void InvalidateBrush()
    {
        _cachedBrush = null;
    }

    private SolidColorBrush GetOrCreateBrush()
    {
        if (_cachedBrush == null)
        {
            _cachedBrush = new SolidColorBrush(GridlineColor)
            {
                Opacity = GridlineOpacity
            };
        }
        return _cachedBrush;
    }

    private Line GetOrCreateLine()
    {
        if (_linePoolIndex < _linePool.Count)
        {
            return _linePool[_linePoolIndex++];
        }

        var line = new Line
        {
            StrokeThickness = 1
        };
        _linePool.Add(line);
        _linePoolIndex++;
        return line;
    }

    private void DrawGridlines()
    {
        // Reset line pool index to reuse existing lines
        _linePoolIndex = 0;

        // Clear children but keep lines in the pool
        Children.Clear();

        if (ActualWidth <= 0 || ActualHeight <= 0 || GridSpacing <= 0)
        {
            return;
        }

        var brush = GetOrCreateBrush();

        // Draw vertical lines
        for (double x = GridSpacing; x < ActualWidth; x += GridSpacing)
        {
            var line = GetOrCreateLine();
            line.X1 = x;
            line.Y1 = 0;
            line.X2 = x;
            line.Y2 = ActualHeight;
            line.Stroke = brush;

            if (GridlineStrokeDashArray != null && GridlineStrokeDashArray.Count > 0)
            {
                line.StrokeDashArray = GridlineStrokeDashArray;
            }
            else
            {
                line.StrokeDashArray = null;
            }

            Children.Add(line);
        }

        // Draw horizontal lines
        for (double y = GridSpacing; y < ActualHeight; y += GridSpacing)
        {
            var line = GetOrCreateLine();
            line.X1 = 0;
            line.Y1 = y;
            line.X2 = ActualWidth;
            line.Y2 = y;
            line.Stroke = brush;

            if (GridlineStrokeDashArray != null && GridlineStrokeDashArray.Count > 0)
            {
                line.StrokeDashArray = GridlineStrokeDashArray;
            }
            else
            {
                line.StrokeDashArray = null;
            }

            Children.Add(line);
        }
    }
}
