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
    private const double MinSpacing = 10;
    private const double MaxSpacing = 100;
    private const double SpacingIncrement = 10;

    /// <summary>
    /// Identifies the GridSpacing dependency property.
    /// </summary>
    public static readonly DependencyProperty GridSpacingProperty =
        DependencyProperty.Register(
            nameof(GridSpacing),
            typeof(double),
            typeof(GridlinesOverlay),
            new PropertyMetadata(MinSpacing, OnGridSpacingChanged));

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
    /// Gets or sets the spacing between gridlines.
    /// </summary>
    public double GridSpacing
    {
        get => (double)GetValue(GridSpacingProperty);
        set => SetValue(GridSpacingProperty, value);
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
    /// </summary>
    public double GridlineOpacity
    {
        get => (double)GetValue(GridlineOpacityProperty);
        set => SetValue(GridlineOpacityProperty, value);
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
            // Ctrl+G: Toggle visibility
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            e.Handled = true;
        }
        else if (e.Key == VirtualKey.G && Visibility == Visibility.Visible)
        {
            // G: Increase spacing
            var newSpacing = GridSpacing + SpacingIncrement;
            if (newSpacing > MaxSpacing)
            {
                // Hide when exceeding max spacing
                Visibility = Visibility.Collapsed;
                GridSpacing = MinSpacing;
            }
            else
            {
                GridSpacing = newSpacing;
            }
            e.Handled = true;
        }
    }

    private static void OnGridSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            overlay.DrawGridlines();
        }
    }

    private static void OnGridlineColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
            overlay.DrawGridlines();
        }
    }

    private static void OnGridlineOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridlinesOverlay overlay)
        {
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

    private void DrawGridlines()
    {
        Children.Clear();

        if (ActualWidth <= 0 || ActualHeight <= 0 || GridSpacing <= 0)
        {
            return;
        }

        var brush = new SolidColorBrush(GridlineColor)
        {
            Opacity = GridlineOpacity
        };

        // Draw vertical lines
        for (double x = GridSpacing; x < ActualWidth; x += GridSpacing)
        {
            var line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = ActualHeight,
                Stroke = brush,
                StrokeThickness = 1
            };

            if (GridlineStrokeDashArray != null && GridlineStrokeDashArray.Count > 0)
            {
                line.StrokeDashArray = GridlineStrokeDashArray;
            }

            Children.Add(line);
        }

        // Draw horizontal lines
        for (double y = GridSpacing; y < ActualHeight; y += GridSpacing)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = ActualWidth,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = 1
            };

            if (GridlineStrokeDashArray != null && GridlineStrokeDashArray.Count > 0)
            {
                line.StrokeDashArray = GridlineStrokeDashArray;
            }

            Children.Add(line);
        }
    }
}
