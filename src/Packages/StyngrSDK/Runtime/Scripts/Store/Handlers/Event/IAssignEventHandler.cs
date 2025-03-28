using Styngr.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event
{
    public interface IAssignEventHandler<T>
    {
        IEnumerator PopulateData(Action<List<T>> onSuccess, Action<ErrorInfo> onFail);

        IEnumerator Bind();
    }
}
