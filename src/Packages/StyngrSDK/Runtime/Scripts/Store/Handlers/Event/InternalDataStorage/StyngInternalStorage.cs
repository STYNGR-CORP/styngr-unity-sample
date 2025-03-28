using Styngr.Exceptions;
using Styngr.Model.Styngs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage
{
    /// <summary>
    /// Internal storage used for <see cref="Styng"/> values.
    /// </summary>
    /// <remarks>
    /// Storage keeps only <see cref="Styng"/> objects that represent Styngs that were bought by the user.
    /// </remarks>
    public class StyngInternalStorage : IStorage<Styng>
    {
        private List<Styng> values;

        /// <summary>
        /// Gets the <see cref="Styng"/> data.
        /// </summary>
        /// <param name="onSuccess">Invoked on successful response.</param>
        /// <param name="onFail">Invoked on failed response.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator GetData(Action<List<Styng>> onSuccess, Action<ErrorInfo> onFail)
        {
            if (values == null || values.Count == 0)
            {
                var actionCallback = new Action<StyngCollection>((styngs) =>
                {
                    values = styngs.Items;
                    onSuccess(values);
                });

                yield return StoreManager.Instance.StoreInstance.GetMyStyngs(actionCallback, onFail);
            }
            else
            {
                onSuccess(values);
            }
        }
    }
}
