using Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage;
using Styngr.Exceptions;
using Styngr.Model.Styngs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event
{
    public class StyngAssignEventHandler : IAssignEventHandler<Styng>
    {
        private readonly IStorage<Styng> storage;

        public StyngAssignEventHandler()
        {
            storage = new StyngInternalStorage();
        }

        public IEnumerator Bind()
        {
            throw new NotImplementedException();
        }

        public IEnumerator PopulateData(Action<List<Styng>> onSuccess, Action<ErrorInfo> onFail)
        {
            yield return storage.GetData(onSuccess, onFail);
        }
    }
}
