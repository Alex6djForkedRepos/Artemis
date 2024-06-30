﻿using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleaseItemViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopService _workshopService;
    private readonly IEntryDetails _entry;
    [Notify] private bool _isCurrentVersion;

    public EntryReleaseItemViewModel(IWorkshopService workshopService, IEntryDetails entry, IRelease release)
    {
        _workshopService = workshopService;
        _entry = entry;

        Release = release;

        this.WhenActivated(d =>
        {
            _workshopService.OnEntryInstalled += WorkshopServiceOnOnEntryInstalled;
            _workshopService.OnEntryUninstalled += WorkshopServiceOnOnEntryInstalled;
            Disposable.Create(() =>
            {
                _workshopService.OnEntryInstalled -= WorkshopServiceOnOnEntryInstalled;
                _workshopService.OnEntryUninstalled -= WorkshopServiceOnOnEntryInstalled;
            }).DisposeWith(d);

            IsCurrentVersion = _workshopService.GetInstalledEntry(_entry.Id)?.ReleaseId == Release.Id;
        });
    }

    public IRelease Release { get; }

    private void WorkshopServiceOnOnEntryInstalled(object? sender, InstalledEntry e)
    {
        IsCurrentVersion = _workshopService.GetInstalledEntry(_entry.Id)?.ReleaseId == Release.Id;
    }
}