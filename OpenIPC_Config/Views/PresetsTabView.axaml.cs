using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class PresetsTabView : UserControl
{
    public PresetsTabView()
    {
        InitializeComponent();
        
        //if (!Design.IsDesignMode) DataContext = new PresetsTabViewModel();
        DataContext = App.ServiceProvider.GetService<PresetsTabViewModel>();
        
        Console.WriteLine($"DataContext is: {DataContext?.GetType().Name ?? "null"}");
        
    }
}