﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Plugins;
using Humanizer;

namespace Artemis.Core;

/// <summary>
///     Represents basic info about a plugin feature and contains a reference to the instance of said feature
/// </summary>
public class PluginFeatureInfo : IPrerequisitesSubject
{
    internal PluginFeatureInfo(Plugin plugin, Type featureType, PluginFeatureEntity pluginFeatureEntity, PluginFeatureAttribute? attribute)
    {
        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        FeatureType = featureType ?? throw new ArgumentNullException(nameof(featureType));
        Entity = pluginFeatureEntity;

        Name = attribute?.Name ?? featureType.Name.Humanize(LetterCasing.Title);
        Description = attribute?.Description;
        AlwaysEnabled = attribute?.AlwaysEnabled ?? false;
    }

    internal PluginFeatureInfo(Plugin plugin, PluginFeatureAttribute? attribute, PluginFeature instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        FeatureType = instance.GetType();
        Entity = new PluginFeatureEntity();

        Name = attribute?.Name ?? instance.GetType().Name.Humanize(LetterCasing.Title);
        Description = attribute?.Description;
        AlwaysEnabled = attribute?.AlwaysEnabled ?? false;
        Instance = instance;
    }

    /// <summary>
    ///     Gets the plugin this feature info is associated with
    /// </summary>
    public Plugin Plugin { get; }

    /// <summary>
    ///     Gets the type of the feature
    /// </summary>
    public Type FeatureType { get; }

    /// <summary>
    ///     Gets the exception thrown while loading
    /// </summary>
    public Exception? LoadException { get; internal set; }

    /// <summary>
    ///     The name of the feature
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     A short description of the feature
    /// </summary>
    public string? Description { get; }

    /// <summary>
    ///     Marks the feature to always be enabled as long as the plugin is enabled and cannot be disabled.
    ///     <para>Note: always <see langword="true" /> if this is the plugin's only feature</para>
    /// </summary>
    public bool AlwaysEnabled { get; internal set; }

    /// <summary>
    ///     Gets a boolean indicating whether the feature is enabled in persistent storage
    /// </summary>
    public bool EnabledInStorage => Entity.IsEnabled;

    /// <summary>
    ///     Gets the feature this info is associated with
    ///     <para>Note: <see langword="null" /> if the associated <see cref="Plugin" /> is disabled</para>
    /// </summary>
    public PluginFeature? Instance { get; internal set; }

    /// <inheritdoc />
    public List<PluginPrerequisite> Prerequisites { get; } = new();

    /// <inheritdoc />
    public IEnumerable<PluginPrerequisite> PlatformPrerequisites => Prerequisites.Where(p => p.AppliesToPlatform());

    /// <inheritdoc />
    public bool ArePrerequisitesMet()
    {
        return PlatformPrerequisites.All(p => p.IsMet());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Instance?.Id ?? "Uninitialized feature";
    }

    /// <summary>
    ///     Returns a boolean indicating whether this feature info matches the provided search string
    /// </summary>
    /// <param name="search">The search string to match</param>
    /// <returns>A boolean indicating whether this plugin info matches the provided search string</returns>
    public bool MatchesSearch(string search)
    {
        return Name.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
               (Description != null && Description.Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }

    internal PluginFeatureEntity Entity { get; }
}