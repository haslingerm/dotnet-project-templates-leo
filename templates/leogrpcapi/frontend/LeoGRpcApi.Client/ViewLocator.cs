using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LeoGRpcApi.Client.ViewModels;

namespace LeoGRpcApi.Client;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        string name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control) Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = $"Not Found: {name}" };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
