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

    /// <summary>
    /// Identifies the DefaultSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty DefaultSpacingProperty =
        DependencyProperty.Register(
            nameof(DefaultSpacing),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(8.0));

    /// <summary>
    /// Identifies the MinSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty MinSpacingProperty =
        DependencyProperty.Register(
            nameof(MinSpacing),
            typeof(double?),
            typeof(GridlinesOverlay),
            new PropertyMetadata(null));

    /// <summary>
    /// Identifies the MaxSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxSpacingProperty =
        DependencyProperty.Register(
            nameof(MaxSpacing),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(64.0));

    /// <summary>
    /// Identifies the SpacingIncrement dependency property.
    /// </summary>
    public static readonly DependencyProperty SpacingIncrementProperty =
        DependencyProperty.Register(
            nameof(SpacingIncrement),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(8.0));

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
    /// Gets or sets the default spacing used when the gridlines are made visible.
    /// Must be a positive value.
    /// </summary>
    public double DefaultSpacing
    {
        get => (double)GetValue(DefaultSpacingProperty);
        set
        {
            // Validate that spacing is positive
            if (value > 0)
            {
                SetValue(DefaultSpacingProperty, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum spacing used when cycling through the gridline spacing.
    /// If null, the DefaultSpacing value is used.
    /// </summary>
    public double? MinSpacing
    {
        get => (double?)GetValue(MinSpacingProperty);
        set
        {
            // Validate that spacing is positive when not null
            if (value.HasValue && value.Value <= 0)
            {
                return; // Silently ignore invalid values
            }
            SetValue(MinSpacingProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the maximum spacing that can be used when cycling through the gridline spacing.
    /// Must be a positive value.
    /// </summary>
    public double MaxSpacing
    {
        get => (double)GetValue(MaxSpacingProperty);
        set
        {
            // Validate that spacing is positive
            if (value > 0)
            {
                SetValue(MaxSpacingProperty, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the increment used when cycling through the gridline spacing.
    /// Must be a positive value.
    /// </summary>
    public double SpacingIncrement
    {
        get => (double)GetValue(SpacingIncrementProperty);
        set
        {
            // Validate that spacing is positive
            if (value > 0)
            {
                SetValue(SpacingIncrementProperty, value);
            }
        }
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

    public GridlinesOverlay()
    {
        IsHitTestVisible = false;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find the root element to attach keyboard handler
        if (XamlRoot?.Content is UIElement rootElement)
        {
            rootElement.KeyDown += OnRootKeyDown;
        }
        DrawGridlines();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Remove keyboard handler
        if (XamlRoot?.Content is UIElement rootElement)
        {
            rootElement.KeyDown -= OnRootKeyDown;
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawGridlines();
    }

    private void OnRootKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var isCtrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (isCtrlPressed && e.Key == VirtualKey.G)
        {
            if (Visibility == Visibility.Collapsed)
            {
                // Ctrl+G when hidden: Show with default spacing
                GridSpacing = DefaultSpacing;
                Visibility = Visibility.Visible;
            }
            else
            {
                // Ctrl+G when visible: Increase spacing by increment, cycling through min-to-max
                var minSpacing = MinSpacing ?? DefaultSpacing;
                var newSpacing = GridSpacing + SpacingIncrement;
                if (newSpacing > MaxSpacing)
                {
                    // Once max is reached, reset to minimum and continue
                    GridSpacing = minSpacing;
                }
                else
                {
                    GridSpacing = newSpacing;
                }
            }
            e.Handled = true;
        }
        else if (e.Key == VirtualKey.G && !isCtrlPressed)
        {
            if (Visibility == Visibility.Visible)
            {
                // G (without Ctrl) when visible: Hide gridlines
                Visibility = Visibility.Collapsed;
            }
            e.Handled = true;
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
                // Avoid recursion by only setting if value is different from default
                overlay.SetValue(GridSpacingProperty, overlay.DefaultSpacing);
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
