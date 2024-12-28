using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace OpenIPC_Config.ViewModels;

public class OverlayItem : INotifyPropertyChanged
{
    private double _positionX;
    private double _positionY;
    private bool _isDragging;
    private string _displayValue;
    
    public string Name { get; set; }
    public double PositionX
    {
        get => _positionX;
        set
        {
            _positionX = value;
            OnPropertyChanged();
        }
    }

    public double PositionY
    {
        get => _positionY;
        set
        {
            _positionY = value;
            OnPropertyChanged();
        }
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                Console.WriteLine($"IsVisible changed to: {_isVisible}");
                OnPropertyChanged();
            }
        }
    }
    public bool IsDragging
    {
        get => _isDragging;
        set
        {
            _isDragging = value;
            OnPropertyChanged();
        }
    }

    public string DisplayValue
    {
        get => _displayValue;
        set
        {
            if (_displayValue != value)
            {
                _displayValue = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    
}

