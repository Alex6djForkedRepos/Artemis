﻿using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.UI.Screens.Debugger.DataModel;
using Artemis.UI.Screens.Debugger.Logs;
using Artemis.UI.Screens.Debugger.Performance;
using Artemis.UI.Screens.Debugger.Render;
using Artemis.UI.Screens.Debugger.Routing;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger;

public partial class DebugViewModel : ActivatableViewModelBase, IScreen
{
    [Notify] private ViewModelBase _selectedItem;

    public DebugViewModel(IDebugService debugService, RenderDebugViewModel render, DataModelDebugViewModel dataModel, PerformanceDebugViewModel performance, RoutingDebugViewModel routing, LogsDebugViewModel logs)
    {
        Items = new ObservableCollection<ViewModelBase> {render, dataModel, performance, routing, logs};
        _selectedItem = render;

        this.WhenActivated(d => Disposable.Create(debugService.ClearDebugger).DisposeWith(d));
    }

    public ObservableCollection<ViewModelBase> Items { get; }
    
    public void Activate()
    {
        OnActivationRequested();
    }

    public event EventHandler? ActivationRequested;

    protected virtual void OnActivationRequested()
    {
        ActivationRequested?.Invoke(this, EventArgs.Empty);
    }

    public RoutingState Router { get; } = new();
}