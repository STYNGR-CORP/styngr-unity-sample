using Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage;
using Styngr.Exceptions;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Event.InternalDataStorage
{
    /// <summary>
    /// Internal storage used for <see cref="NFT"/> values.
    /// </summary>
    /// <remarks>
    /// Storage keeps only <see cref="NFT"/> objects that represent NFTs that were bought by the user.
    /// </remarks>
    public class NFTInternalStorage : IStorage<NFT>
    {
        private List<NFT> values;

        /// <summary>
        /// Gets the <see cref="NFT"/> data.
        /// </summary>
        /// <param name="onSuccess">Invoked on successful response.</param>
        /// <param name="onFail">Invoked on failed response.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator GetData(Action<List<NFT>> onSuccess, Action<ErrorInfo> onFail)
        {
            if (values == null || values.Count == 0)
            {
                var actionCallback = new Action<NFTsCollection>((nfts) =>
                {
                    values = nfts.Items.ToList();
                    onSuccess(values);
                });

                yield return StoreManager.Instance.StoreInstance.GetMyNfts(actionCallback, onFail);
            }
            else
            {
                onSuccess(values);
                yield break;
            }
        }
    }
}
