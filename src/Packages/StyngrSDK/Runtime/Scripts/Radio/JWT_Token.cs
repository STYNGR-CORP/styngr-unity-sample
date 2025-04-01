using Newtonsoft.Json;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using static Styngr.StyngrSDK;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio
{
    /// <summary>
    /// Contains all the data required for getting a valid token.
    /// NOTE: Data should be filled before proceeding.
    /// </summary>
    public class JWT_Token : MonoBehaviour
    {
        private IEnumerator GetTokenPtr;

        /// <summary>
        /// Triggers when token is configured
        /// </summary>
        private EventHandler<string> onTokenReady;

        [SerializeField] private TextAsset tokenConfigurationJSON;

        public static string Token { get; private set; } = string.Empty;

        public static JWT_Token Main { get; private set; } = null;

        /// <summary>
        /// For special configuration overrides (usually not used).
        /// </summary>
        public static TextAsset TokenConfigurationJSONOverride { get; set; }

        /// <summary>
        /// JSON configuration with host and x api token
        /// </summary>
        public static ConfigurationJSON JsonConfig { get; private set; }

        /// <summary>
        /// Add only one call per method to the event handler
        /// </summary>
        public event EventHandler<string> OnTokenReady
        {
            add
            {
                if (onTokenReady == null || !onTokenReady.GetInvocationList().Contains(value))
                {
                    onTokenReady += value;
                }
            }
            remove
            {
                onTokenReady -= value;
            }
        }

        /// <summary>
        /// Gets the token which will be used throughout whole application.
        /// </summary>
        /// <param name="sender">Origin of the call.</param>
        /// <param name="configurationJson">Configuration with required parameters.</param>
        public IEnumerator GetToken(object sender, TextAsset configurationJson)
        {
            yield return new WaitForEndOfFrame();

            Debug.Log($"OS name: {OperatingSystemInfo.OperatingSystemName}");
            Debug.Log($"OS version: {OperatingSystemInfo.OperatingSystemVersion}");
            Debug.Log($"Detailed OS version: {OperatingSystemInfo.DetailedOperatingSystemVersion}");

            JsonConfig = JsonConvert.DeserializeObject<ConfigurationJSON>(configurationJson.text);
            string body = JsonConvert.SerializeObject(JsonConfig.GetBody());
            Debug.Log("Getting Styngr token");

            void callback(JWT jwt)
            {
                Token = jwt.Token;

                if (Main != null && Main.onTokenReady != null)
                {
                    Main.onTokenReady(sender, jwt.Token);
                }

                Debug.Log("Getting Styngr token finished");
                StartCoroutine(LogUserSubscription());
            }

            void errorCallback(ErrorInfo errorInfo)
            {
                Debug.LogError(errorInfo.Errors);
            }

            SetHost(JsonConfig.host);

            StartCoroutine(Styngr.StyngrSDK.GetToken(body, JsonConfig.XApiToken, onSuccess: callback, onFail: errorCallback));
        }

        private void Awake() =>
            Main = this;

        // Start is called before the first frame update
        private void Start()
        {
            if (GetTokenPtr != null)
            {
                StopCoroutine(GetTokenPtr);
            }

            GetTokenPtr = TokenConfigurationJSONOverride == null ? GetToken(this, tokenConfigurationJSON) : GetToken(this, TokenConfigurationJSONOverride);
            StartCoroutine(GetTokenPtr);
        }

        private IEnumerator LogUserSubscription()
        {
            yield return new WaitUntil(() => StoreManager.Instance.IsSuccess());
            yield return StoreManager.Instance.StoreInstance.GetActiveUserSubscription(Token, LogOnSuccess, (errorInfo) => Debug.LogWarning(errorInfo.Errors));
        }

        private void LogOnSuccess(ActiveSubscription activeSubscription) =>
            Debug.Log($"Currently active user subscription:{Environment.NewLine}{activeSubscription}");
    }
}