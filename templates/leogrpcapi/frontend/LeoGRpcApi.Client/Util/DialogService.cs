using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using LeoGRpcApi.Client.ViewModels;

namespace LeoGRpcApi.Client.Util;

/// <summary>
///     Allows to show modal dialogs based on a window and view model.
///     Each dialog can return a result (or null) when closed.
/// </summary>
public interface IDialogService
{
    /// <summary>
    ///     Shows a modal dialog window with the specified view model.
    ///     The owner window must be set before calling this method, see <see cref="SetOwner" />.
    /// </summary>
    /// <param name="viewModelConfiguration">Callback for configuring the view model after creation</param>
    /// <typeparam name="TDialogWindow">Type of the dialog window</typeparam>
    /// <typeparam name="TDialogViewModel">Type of the dialog view model</typeparam>
    /// <typeparam name="TResult">Return value of the dialog</typeparam>
    /// <returns>An awaitable Task with the result of the dialog</returns>
    public Task<TResult> ShowModalDialogAsync<TDialogWindow, TDialogViewModel, TResult>(
        Action<TDialogViewModel>? viewModelConfiguration = null)
        where TDialogWindow : Window, new()
        where TDialogViewModel : DialogViewModelBase<TResult>;

    /// <summary>
    ///     Sets the owner window for the dialogs.
    ///     In this application that is always the main window, so this method should not be called outside the setup phase
    /// </summary>
    /// <param name="window">The parent window of the dialog windows</param>
    public void SetOwner(Window window);
}

internal sealed class DialogService : IDialogService
{
    private Window? _owner;

    public async Task<TResult> ShowModalDialogAsync<TDialogWindow, TDialogViewModel, TResult>(
        Action<TDialogViewModel>? viewModelConfiguration = null)
        where TDialogWindow : Window, new()
        where TDialogViewModel : DialogViewModelBase<TResult>
    {
        if (_owner is null)
        {
            throw new
                InvalidOperationException($"Owner window is not set. Call {nameof(SetOwner)} before showing dialogs.");
        }

        var dialog = new TDialogWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SystemDecorations = SystemDecorations.None,
            CanResize = false,
            ShowInTaskbar = false,
            SizeToContent = SizeToContent.WidthAndHeight
        };
        SetBorder(dialog);

        var viewModel = App.CreateViewModel<TDialogViewModel>();
        viewModel.CloseRequested += (_, result) => { Dispatcher.UIThread.Invoke(() => dialog.Close(result)); };
        viewModelConfiguration?.Invoke(viewModel);
        dialog.DataContext = viewModel;

        var result = await dialog.ShowDialog<TResult>(_owner);

        return result;
    }

    public void SetOwner(Window window)
    {
        _owner = window;
    }

    private static void SetBorder(ContentControl dialog)
    {
        const double MaxExtent = 12;
        const double Delta = 4;
        const double Offset = MaxExtent - Delta;
        const double Padding = MaxExtent + Delta;
        const double Spread = (Delta / 2) * -1;
        var cornerRadius = new CornerRadius(6);
        var lineThickness = new Thickness(1);
        var shadowColor = new Color(125, 0, 0, 0);
        var borderColor = Brushes.LightGray;

        IBrush? background = null;
        if (dialog is Window window)
        {
            window.CornerRadius = cornerRadius;
            background = window.Background;
            window.Background = null;
        }
        
        var border = new Border
        {
            BorderBrush = borderColor,
            BorderThickness = lineThickness,
            CornerRadius = cornerRadius,
            Margin = new Thickness(MaxExtent),
            Padding = new Thickness(Padding),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = Offset,
                OffsetY = Offset,
                Blur = MaxExtent,
                Spread = Spread,
                Color = shadowColor
            }),
            Background = background
        };
        
        object? content = dialog.Content;
        dialog.Content = border;
        border.Child = content as Control;
    }
}
