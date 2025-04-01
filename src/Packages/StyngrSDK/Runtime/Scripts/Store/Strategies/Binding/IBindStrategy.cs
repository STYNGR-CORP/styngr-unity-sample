using Styngr.Interfaces;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Binding
{
    /// <summary>
    /// Strategy for the binding.
    /// </summary>
    internal interface IBindStrategy
    {
        /// <summary>
        /// Binds the event to the specified product.
        /// </summary>
        /// <param name="eventData">The game event data.</param>
        /// <param name="product">The product data.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator Bind(GameEvent eventData, IProduct product);

        /// <summary>
        /// Binds the events to the specified product.
        /// </summary>
        /// <param name="eventsToToggleStates">Collection of the events that should be bind/unbind from the product.</param>
        /// <param name="product">The product data.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator BindMultiple(IEnumerable<Tuple<GameEvent, bool>> eventsToToggleStates, IProduct product);
    }
}
