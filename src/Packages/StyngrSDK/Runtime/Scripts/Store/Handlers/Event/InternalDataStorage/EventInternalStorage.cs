using Styngr.Exceptions;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage
{
    /// <summary>
    /// Internal storage used for game event values.
    /// </summary>
    /// <remarks>
    /// Storage keeps only game events that have bound products.
    /// </remarks>
    public class EventInternalStorage : IStorage<GameEvent>
    {
        private List<GameEvent> values;

        /// <summary>
        /// Gets the game event data.
        /// </summary>
        /// <param name="onSuccess">Invoked on successful response.</param>
        /// <param name="onFail">Invoked on failed response.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator GetData(Action<List<GameEvent>> onSuccess, Action<ErrorInfo> onFail)
        {
            if (values == null || values.Count == 0)
            {
                values = new List<GameEvent>();
                var callbackAction = new Action<GameEvent[]>((gameEvents) =>
                {
                    values.AddRange(gameEvents.ToList());
                });

                yield return StoreManager.Instance.StoreInstance.GetBoundStyngEvents(callbackAction, onFail);
                yield return StoreManager.Instance.StoreInstance.GetBoundNftEvents(callbackAction, onFail);
                onSuccess(values);
            }
            else
            {
                onSuccess(values);
                yield break;
            }
        }
    }
}
