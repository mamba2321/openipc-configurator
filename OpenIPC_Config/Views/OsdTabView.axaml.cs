using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace OpenIPC_Config.Views;

public partial class OsdTabView : UserControl
{
    private Point _lastPointerPosition;
    private Line? _verticalGuide;
    private Line? _horizontalGuide;
    
    private const double SnapThreshold = 10.0;
    private const double GridSize = 20.0; // Set your grid size here
    
    public OsdTabView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetService<OsdTabViewModel>();
    }
    
    private void Control_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is OverlayItem item)
        {
            item.IsDragging = true;
            _lastPointerPosition = e.GetPosition(this);

            // set movement box
            ((Border)sender).Background = Brushes.DarkOrange;
            
            
            if (_verticalGuide == null)
            {
                _verticalGuide = new Line
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    ZIndex = 10
                };
                OsdCanvas.Children.Add(_verticalGuide);
            }

            if (_horizontalGuide == null)
            {
                _horizontalGuide = new Line
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    ZIndex = 10
                };
                OsdCanvas.Children.Add(_horizontalGuide);
            }
        }
    }

    private void Control_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is Border border && border.DataContext is OverlayItem item && item.IsDragging)
        {
            //TODO: should HORIZON move around? Fow now it is simple enable/disable
            if (((OpenIPC_Config.ViewModels.OverlayItem)((Avalonia.Controls.Border)sender).DataContext).Name ==
                "HORIZON")
                return;
                
            var currentPointerPosition = e.GetPosition(this);
            var deltaX = currentPointerPosition.X - _lastPointerPosition.X;
            var deltaY = currentPointerPosition.Y - _lastPointerPosition.Y;

            item.PositionX += deltaX;
            item.PositionY += deltaY;
            
            Log.Verbose($"X: {item.PositionX}, Y: {item.PositionY}");
            
            _lastPointerPosition = currentPointerPosition;

            // Snap to grid
            //SnapToGrid(item);
            
            //UpdateAlignmentHelpers(item.PositionX, item.PositionY);
            UpdateAlignmentHelpers(item.PositionX, item.PositionY, border.Bounds.Width, border.Bounds.Height);

            
        }
    }

    private void Control_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Border border && border.DataContext is OverlayItem item)
        {
            item.IsDragging = false;

            // Snap to grid or guides on release
            SnapToGrid(item);
            
            if (_verticalGuide != null)
            {
                OsdCanvas.Children.Remove(_verticalGuide);
                _verticalGuide = null;
            }

            if (_horizontalGuide != null)
            {
                OsdCanvas.Children.Remove(_horizontalGuide);
                _horizontalGuide = null;
            }
            
            ((Border)sender).Background = Brushes.Transparent;
        }
    }

    private void UpdateAlignmentHelpers(double positionX, double positionY, double itemWidth, double itemHeight)
    {
        // Calculate the center of the item
        var centerX = positionX + (itemWidth / 2);
        var centerY = positionY + (itemHeight / 2);

        // Check alignment with grid
        bool isOnVerticalGrid = Math.Abs(centerX % GridSize) < SnapThreshold;
        bool isOnHorizontalGrid = Math.Abs(centerY % GridSize) < SnapThreshold;

        // Check alignment with canvas center
        double canvasCenterX = OsdCanvas.Bounds.Width / 2;
        double canvasCenterY = OsdCanvas.Bounds.Height / 2;

        bool isCenteredVertically = Math.Abs(centerX - canvasCenterX) < SnapThreshold;
        bool isCenteredHorizontally = Math.Abs(centerY - canvasCenterY) < SnapThreshold;

        // Update vertical guide color
        if (_verticalGuide != null)
        {
            _verticalGuide.StartPoint = new Point(centerX, 0);
            _verticalGuide.EndPoint = new Point(centerX, OsdCanvas.Bounds.Height);

            if (isCenteredVertically)
            {
                _verticalGuide.Stroke = Brushes.OrangeRed; // Aligned to center
            }
            else if (isOnVerticalGrid)
            {
                _verticalGuide.Stroke = Brushes.Blue; // Snapped to grid
            }
            else
            {
                _verticalGuide.Stroke = Brushes.White; // Default
            }
        }

        // Update horizontal guide color
        if (_horizontalGuide != null)
        {
            _horizontalGuide.StartPoint = new Point(0, centerY);
            _horizontalGuide.EndPoint = new Point(OsdCanvas.Bounds.Width, centerY);

            if (isCenteredHorizontally)
            {
                _horizontalGuide.Stroke = Brushes.OrangeRed; // Aligned to center
            }
            else if (isOnHorizontalGrid)
            {
                _horizontalGuide.Stroke = Brushes.Blue; // Snapped to grid
            }
            else
            {
                _horizontalGuide.Stroke = Brushes.White; // Default
            }
        }
    }


    private void SnapToGrid(OverlayItem item)
    {
        item.PositionX = Math.Round(item.PositionX / GridSize) * GridSize;
        item.PositionY = Math.Round(item.PositionY / GridSize) * GridSize;
    }
    
    
}