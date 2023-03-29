using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Shared.DefaultTypes.DataModel.Display;

/// <summary>
///     Represents a default data model display view.
/// </summary>
public partial class DefaultDataModelDisplayView : UserControl
{
    /// <summary>
    ///     Creates a new instance of the <see cref="DefaultDataModelDisplayView" /> class.
    /// </summary>
    public DefaultDataModelDisplayView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}