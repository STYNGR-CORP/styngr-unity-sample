using Styngr.Exceptions;
using System;
using System.Net;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class UI_ErrorsHandler : MonoBehaviour
    {
        public Plug_TryAgain tryAgainContent;
        public Plug_Sceleton waitContent;
        public Plug_BackToGame backToGame;
        public PopUp redAlert;

        public void ShowWaitContent(int sceletonCount = -1)
        {
            if (waitContent != null) waitContent.Show(sceletonCount);
            if (tryAgainContent != null) tryAgainContent.HideImmediate();
        }

        public void ShowTryAgainContent(Action action)
        {
            if (waitContent != null) waitContent.HideImmediate();
            if (tryAgainContent != null) tryAgainContent.Show(action);
        }

        public void ShowTryAgainContentSafe(Action action)
        {
            void a()
            {
                if (waitContent != null) waitContent.HideImmediate();
                if (tryAgainContent != null) tryAgainContent.Show(action);
            }
            StoreManager.Instance.Async.Enqueue(a);
        }

        public void ShowRedAlertSafe()
        {
            void a()
            {
                if (redAlert != null) redAlert.ShowImmediate();
            }
            StoreManager.Instance.Async.Enqueue(a);
        }

        public void HideContentDelayed(int frames = 0)
        {
            if (waitContent != null)
            {
                waitContent.HideDelayed(frames);
            }

            if (tryAgainContent != null)
            {
                tryAgainContent.HideDelayed(frames);
            }
        }

        public void HideContent()
        {
            void a()
            {
                HideContentImmediate();
            }
            StoreManager.Instance.Async.Enqueue(a);
        }

        public void HideContentImmediate()
        {
            if (waitContent != null)
            {
                waitContent.HideImmediate();
            }

            if (tryAgainContent != null)
            {
                tryAgainContent.HideImmediate();
            }
        }

        public bool OnError(ErrorInfo errorInfo, Action tryAgainAction)
        {
            if (errorInfo == null)
            {
                return true;
            }
            else
            {
                if (!errorInfo.OK)
                {
                    // If an authorization error has arrived
                    if (errorInfo.httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (backToGame != null)
                        {
                            backToGame.ShowSafe();
                        }
                    }
                    else
                    {
                        // Let's show the Red Alert PopUp
                        ShowRedAlertSafe();

                        // Let's show the Try Again button
                        ShowTryAgainContentSafe(tryAgainAction);
                    }
                }
            }

            return !errorInfo.OK;
        }
    }
}
