﻿using System;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace Artemis.Core.Services;

/// <summary>
///     A service that provides access to the local Artemis web server
/// </summary>
public interface IWebServerService : IArtemisService
{
    /// <summary>
    ///     Gets the current instance of the web server, replaced when <see cref="WebServerStarting" /> occurs.
    /// </summary>
    IServer? Server { get; }

    /// <summary>
    ///     Gets the plugins module containing all plugin end points
    /// </summary>
    PluginsHandler PluginsHandler { get; }

    /// <summary>
    ///     Adds a new endpoint for the given plugin feature receiving an object of type <typeparamref name="T" />
    ///     <para>Note: Object will be deserialized using JSON.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to be received</typeparam>
    /// <param name="feature">The plugin feature the end point is associated with</param>
    /// <param name="endPointName">The name of the end point, must be unique</param>
    /// <param name="requestHandler"></param>
    /// <returns>The resulting end point</returns>
    JsonPluginEndPoint<T> AddJsonEndPoint<T>(PluginFeature feature, string endPointName, Action<T> requestHandler);

    /// <summary>
    ///     Adds a new endpoint for the given plugin feature receiving an object of type <typeparamref name="T" /> and
    ///     returning any <see cref="object" />.
    ///     <para>Note: Both will be deserialized and serialized respectively using JSON.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to be received</typeparam>
    /// <param name="feature">The plugin feature the end point is associated with</param>
    /// <param name="endPointName">The name of the end point, must be unique</param>
    /// <param name="requestHandler"></param>
    /// <returns>The resulting end point</returns>
    JsonPluginEndPoint<T> AddResponsiveJsonEndPoint<T>(PluginFeature feature, string endPointName, Func<T, object?> requestHandler);
    
    /// <summary>
    ///     Adds a new endpoint for the given plugin feature receiving an a <see cref="string" />.
    /// </summary>
    /// <param name="feature">The plugin feature the end point is associated with</param>
    /// <param name="endPointName">The name of the end point, must be unique</param>
    /// <param name="requestHandler"></param>
    /// <returns>The resulting end point</returns>
    StringPluginEndPoint AddStringEndPoint(PluginFeature feature, string endPointName, Action<string> requestHandler);

    /// <summary>
    ///     Adds a new endpoint for the given plugin feature receiving an a <see cref="string" /> and returning a
    ///     <see cref="string" /> or <see langword="null" />.
    /// </summary>
    /// <param name="feature">The plugin feature the end point is associated with</param>
    /// <param name="endPointName">The name of the end point, must be unique</param>
    /// <param name="requestHandler"></param>
    /// <returns>The resulting end point</returns>
    StringPluginEndPoint AddResponsiveStringEndPoint(PluginFeature feature, string endPointName, Func<string, string?> requestHandler);

    /// <summary>
    ///     Adds a new endpoint for the given plugin feature that handles a raw <see cref="IHttpContext" />.
    ///     <para>
    ///         Note: This requires that you reference the EmbedIO
    ///         <see href="https://www.nuget.org/packages/embedio">Nuget package</see>.
    ///     </para>
    /// </summary>
    /// <param name="feature">The plugin feature the end point is associated with</param>
    /// <param name="endPointName">The name of the end point, must be unique</param>
    /// <param name="requestHandler"></param>
    /// <returns>The resulting end point</returns>
    RawPluginEndPoint AddRawEndPoint(PluginFeature feature, string endPointName, Func<IRequest, Task<IResponse>> requestHandler);

    /// <summary>
    ///     Removes an existing endpoint
    /// </summary>
    /// <param name="endPoint">The end point to remove</param>
    void RemovePluginEndPoint(PluginEndPoint endPoint);

    /// <summary>
    ///     Adds a new Web API controller and restarts the web server
    /// </summary>
    /// <typeparam name="T">The type of Web API controller to add</typeparam>
    WebApiControllerRegistration AddController<T>(PluginFeature feature, string path) where T : class;

    /// <summary>
    ///     Removes an existing Web API controller and restarts the web server
    /// </summary>
    /// <param name="registration">The registration of the controller to remove.</param>
    void RemoveController(WebApiControllerRegistration registration);
    
    /// <summary>
    ///     Occurs when the web server has been created and is about to start. This is the ideal place to add your own modules.
    /// </summary>
    event EventHandler? WebServerStarting;
}