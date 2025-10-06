using Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage;
using Styngr.Exceptions;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event
{
    public class GameEventHandler : IAssignEventHandler<GameEvent>
    {
        private IStorage<GameEvent> storage;

        public GameEventHandler()
        {
            storage = new EventInternalStorage();
        }

        public IEnumerator Bind()
        {
            throw new NotImplementedException();
        }

        public IEnumerator PopulateData(Action<List<GameEvent>> onSuccess, Action<ErrorInfo> onFail)
        {
            yield return storage.GetData(onSuccess, onFail);
        }
    }
}
