using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace EasyDocs.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    protected Window Owner { get; }

    public BaseViewModel(Window owner)
    {
        this.Owner = owner;
    }

    public BaseViewModel()
    {
        Debug.WriteLine($"Is design mode: {Design.IsDesignMode}");

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}