using System;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Dialogs;

public class TimelineSegmentEditViewModel : ContentDialogViewModelBase
{
    public TimelineSegmentEditViewModel(TimeSpan segmentLength)
    {
        SegmentLength = segmentLength.TotalSeconds;
    }

    public double SegmentLength { get; set; }
}