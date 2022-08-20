﻿using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public class DeviceAdaptionHintViewModel : AdaptionHintViewModelBase
{
    public DeviceAdaptionHintViewModel(Layer layer, DeviceAdaptionHint adaptionHint) : base(layer, adaptionHint)
    {
        DeviceAdaptionHint = adaptionHint;
    }

    public DeviceAdaptionHint DeviceAdaptionHint { get; }
}