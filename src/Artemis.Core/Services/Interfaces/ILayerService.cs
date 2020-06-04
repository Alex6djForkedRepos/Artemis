﻿using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.LayerBrush.Abstract;

namespace Artemis.Core.Services.Interfaces
{
    public interface ILayerService : IArtemisService
    {
        /// <summary>
        /// Creates a new layer
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Layer CreateLayer(Profile profile, ProfileElement parent, string name);

        /// <summary>
        ///     Instantiates and adds the <see cref="BaseLayerBrush" /> described by the provided
        ///     <see cref="LayerBrushDescriptor" />
        ///     to the <see cref="Layer" />.
        /// </summary>
        /// <param name="layer">The layer to instantiate the brush for</param>
        /// <returns></returns>
        BaseLayerBrush InstantiateLayerBrush(Layer layer);

        /// <summary>
        /// Removes the layer brush from the provided layer and disposes it
        /// </summary>
        /// <param name="layer"></param>
        void RemoveLayerBrush(Layer layer);
    }
}