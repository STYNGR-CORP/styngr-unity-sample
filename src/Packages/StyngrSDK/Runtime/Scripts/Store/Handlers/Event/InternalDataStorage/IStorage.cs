using Styngr.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage
{
    /// <summary>
    /// Storage interface used to dictate the implementation need of the concrete internal storages.
    /// </summary>
    /// <typeparam name="T">Template which describes object on which the storage operates.</typeparam>
    public interface IStorage<T>
    {
        /// <summary>
        /// Gets the required data.
        /// </summary>
        /// <param name="onSuccess">Invoked on successful response.</param>
        /// <param name="onFail">Invoked on failed response.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator GetData(Action<List<T>> onSuccess, Action<ErrorInfo> onFail);
    }
}
