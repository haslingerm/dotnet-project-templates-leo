using CommunityToolkit.Mvvm.ComponentModel;

namespace LeoGRpcApi.Client.ViewModels;

public abstract class ViewModelBase : ObservableObject;

/// <summary>
///     Base class for dialog view models
/// </summary>
/// <typeparam name="TResult">The result type of the dialog</typeparam>
public abstract class DialogViewModelBase<TResult> : ViewModelBase
{
    /// <summary>
    ///     Called when the dialog is closed, providing the result of the dialog
    /// </summary>
    public event EventHandler<TResult>? CloseRequested;

    /// <summary>
    ///     Closes the dialog and returns the specified result
    /// </summary>
    /// <param name="result"></param>
    protected void Close(TResult result)
    {
        CloseRequested?.Invoke(this, result);
    }
}
