using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    /// <summary>
    /// Store manager singleton keeps account of Styngr SDK Store object through scenes
    /// </summary>
    public class StoreManager : MonoBehaviour
    {
        /// <summary>
        /// StoreManager singleton instance
        /// </summary>
        public static StoreManager Instance;

        /// <summary>
        /// SDK's Store singleton instance
        /// </summary>
        public Styngr.Store StoreInstance { get; private set; }

        /// <summary>
        /// Concurrent queue for asynchronous actions
        /// </summary>
        public ConcurrentQueue<Action> Async = new ConcurrentQueue<Action>();

        /// <summary>
        /// If the Store has Storage for downloading styngs locally
        /// </summary>
        public bool storageEnabled = false;

        /// <summary>
        /// State of StoreInstance
        /// </summary>
        private StoreInstanceState state = StoreInstanceState.IDLE;

        #region Public

        /// <summary>
        /// Check if state is SUCCESS
        /// </summary>
        /// <returns>bool</returns>
        public bool IsSuccess()
        {
            if (Instance == null)
            {
                Debug.LogError("Store Manager is not loaded");
                return false;
            }
            return Instance.state == StoreInstanceState.SUCCESS;
        }

        /// <summary>
        /// Check if state is ERROR
        /// </summary>
        /// <returns>bool</returns>
        public bool IsError()
        {
            if (Instance == null)
            {
                Debug.LogError("Store Manager is not loaded");
                return false;
            }
            return Instance.state == StoreInstanceState.ERROR;
        }

        /// <summary>
        /// Starts store loading process if its possible
        /// </summary>
        /// <param name="sdkToken">Access token</param>
        public void LoadStore(string sdkToken)
        {
            if (!Instance.IsSuccess())
            {
                Instance.StartStoreLoading(sdkToken);
            }
        }

        /// <summary>
        /// Loads single instance of SDK's Store
        /// </summary>
        /// <param name="sdkToken"></param>
        private void LoadStoreInstance(string sdkToken)
        {
            Debug.Log("[StoreManager] loading store instance...");

            Instance.StoreInstance ??= new Styngr.Store();

            Instance.StoreInstance.OnStoreTokenObtained += OnStoreTokenObtained;
            StartCoroutine(Instance.StoreInstance.GetStoreToken(sdkToken));
        }

        #endregion Public

        #region Private

        private void OnStoreTokenObtained(object sender, bool obtained)
        {
            if (obtained)
            {
                SetStoreSuccess();
            }
            else
            {
                SetStoreError();
            }
        }

        private static void SetStoreSuccess()
        {
            if (Instance)
            {
                Instance.state = StoreInstanceState.SUCCESS;
            }
            else
            {
                Debug.Log("SDK Store token is successfully obtained.");
            }
        }

        private static void SetStoreError()
        {
            if (Instance)
            {
                Instance.state = StoreInstanceState.ERROR;
            }
            else
            {
                Debug.LogError("Store Manager is not loaded");
            }
        }

        private void StartStoreLoading(string sdkToken)
        {
            if (Instance.StoreInstance == null)
            {
                StopAllCoroutines();

                Debug.Log("Waiting for Styngr store to load...");

                LoadStoreInstance(sdkToken);
                StartCoroutine(WaitStoreLoad());

                Debug.Log("Styngr store loaded!");
            }
        }

        private IEnumerator WaitStoreLoad()
        {
            yield return new WaitUntil(() => IsSuccess());
        }

        #endregion Private

        #region Unity methods

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (IsSuccess())
            {
                while (Instance.Async.TryDequeue(out Action a))
                {
                    a();
                }
            }
        }

        #endregion Unity methods
    }
}
