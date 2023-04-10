using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;

public partial class StaticConditionView : ReactiveUserControl<StaticConditionViewModel>
{
    public StaticConditionView()
    {
        InitializeComponent();
    }

}