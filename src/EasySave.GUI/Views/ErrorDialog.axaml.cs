using System;
using Avalonia.Controls;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog(Exception ex)
    {
        InitializeComponent();

        var vm = new ErrorDialogViewModel(ex);
        vm.CloseRequested += () => Close();
        DataContext = vm;
    }

    // Parameterless constructor required for XAML designer
    public ErrorDialog() : this(new Exception("Design-time preview")) { }
}