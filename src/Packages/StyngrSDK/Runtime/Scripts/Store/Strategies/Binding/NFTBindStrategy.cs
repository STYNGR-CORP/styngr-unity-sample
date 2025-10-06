using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Interfaces;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Binding
{
    /// <summary>
    /// Binding strategy of the NFTs.
    /// </summary>
    internal class NFTBindStrategy : IBindStrategy
    {
        private readonly Action bindingFinished;

        private GameEvent gameEventData;
        private IProduct tempProduct;

        /// <summary>
        /// Creates an instance of the <see cref="NFTBindStrategy"/> class.
        /// </summary>
        /// <param name="bindingFinished">Action which will be invoked when the binding process has been finished.</param>
        public NFTBindStrategy(Action bindingFinished)
        {
            this.bindingFinished = bindingFinished;
        }

        /// <inheritdoc/>
        public IEnumerator Bind(GameEvent eventData, IProduct product)
        {
            gameEventData = eventData;
            tempProduct = product;

            var hasBoundProduct = gameEventData.TryGetBoundProduct(out var boundProduct);
            var isProductSet = tempProduct != null;

            if (isProductSet)
            {
                if (hasBoundProduct && !gameEventData.AreProductIdsEqual(tempProduct.GetId()))
                {
                    yield return StoreManager.Instance.StoreInstance.UnbindEventAsync(gameEventData, OnUnbindSuccess, OnFail);
                    yield return StoreManager.Instance.StoreInstance.BindEventAsync(ProductType.NFT, tempProduct, gameEventData, OnBindSuccess, OnFail);
                }
                if (!hasBoundProduct)
                {
                    yield return StoreManager.Instance.StoreInstance.BindEventAsync(ProductType.NFT, tempProduct, gameEventData, OnBindSuccess, OnFail);
                }
            }
            else
            {
                if (hasBoundProduct)
                {
                    yield return StoreManager.Instance.StoreInstance.UnbindEventAsync(gameEventData, OnUnbindSuccess, OnFail);
                }
            }
            bindingFinished();
        }

        /// <inheritdoc/>
        public IEnumerator BindMultiple(IEnumerable<Tuple<GameEvent, bool>> eventsToToggleStates, IProduct product)
        {
            foreach (var gameEvent in eventsToToggleStates)
            {
                yield return BindPrivate(gameEvent, product);
            }
            bindingFinished();
        }

        private IEnumerator BindPrivate(Tuple<GameEvent, bool> eventsToToggleState, IProduct product)
        {
            gameEventData = eventsToToggleState.Item1;
            tempProduct = product;

            var hasBoundProduct = gameEventData.TryGetBoundProduct(out var boundProduct);
            var isProductSet = eventsToToggleState.Item2;

            if (isProductSet)
            {
                if (hasBoundProduct && !gameEventData.AreProductIdsEqual(tempProduct.GetId()))
                {
                    yield return StoreManager.Instance.StoreInstance.UnbindEventAsync(gameEventData, OnUnbindSuccess, OnFail);
                    yield return StoreManager.Instance.StoreInstance.BindEventAsync(ProductType.NFT, tempProduct, gameEventData, OnBindSuccess, OnFail);
                }
                if (!hasBoundProduct)
                {
                    yield return StoreManager.Instance.StoreInstance.BindEventAsync(ProductType.NFT, tempProduct, gameEventData, OnBindSuccess, OnFail);
                }
            }
            else
            {
                if (hasBoundProduct && gameEventData.AreProductIdsEqual(tempProduct.GetId()))
                {
                    yield return StoreManager.Instance.StoreInstance.UnbindEventAsync(gameEventData, OnUnbindSuccess, OnFail);
                }
            }
        }

        private void OnUnbindSuccess()
        {
            gameEventData.TryUnbindProduct();
        }

        private void OnBindSuccess()
        {
            var tempEvent = new NFTGameEvent(gameEventData);
            tempEvent.TrySetBoundProduct(tempProduct);
        }

        private void OnFail(ErrorInfo errorInfo)
        {
            if (errorInfo.httpStatusCode == HttpStatusCode.Unauthorized)
            {
                Plug_BackToGame.main.ShowSafe();
            }
            else
            {
                PopUp.main.ShowSafe();
            }
        }
    }
}
