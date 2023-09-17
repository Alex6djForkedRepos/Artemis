﻿using System.Reactive;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.ContentDialogs;

public class ProfileElementRenameViewModel : ContentDialogViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly ProfileElement _profileElement;
    private string? _profileElementName;

    public ProfileElementRenameViewModel(IProfileEditorService profileEditorService, ProfileElement profileElement)
    {
        _profileEditorService = profileEditorService;
        _profileElement = profileElement;
        _profileElementName = profileElement.Name;

        Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);
        this.ValidationRule(vm => vm.ProfileElementName, name => !string.IsNullOrWhiteSpace(name), "You must specify a valid name");
    }

    public string? ProfileElementName
    {
        get => _profileElementName;
        set => RaiseAndSetIfChanged(ref _profileElementName, value);
    }


    public ReactiveCommand<Unit, Unit> Confirm { get; }

    private void ExecuteConfirm()
    {
        if (ProfileElementName == null)
            return;

        _profileEditorService.ExecuteCommand(new RenameProfileElement(_profileElement, ProfileElementName));
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}