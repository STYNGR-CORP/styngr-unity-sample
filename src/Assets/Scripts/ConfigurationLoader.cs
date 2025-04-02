using Assets.Utils.Enums;
using Newtonsoft.Json;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using System;
using System.Collections;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Assets.Scripts
{
    /// <summary>
    /// Configuration loader script used for manual configuration of the JSON configuration.
    /// </summary>
    public class ConfigurationLoader : MonoBehaviour
    {
        private const string HostLabelTag = "HostLabel";
        private const string AppDataConfigName = "Config.bin";
        private const int DefaultInitScene = 1;

        private string appDataConfigPath;

        [SerializeField] private GameObject loadingScreen;

        [SerializeField] private TextAsset jsonConfiguration;

        [SerializeField] private TMP_InputField host;

        [SerializeField] private TMP_InputField expiresIn;

        [SerializeField] private TMP_InputField userId;

        [SerializeField] private TMP_InputField appId;

        [SerializeField] private TMP_InputField deviceId;

        [SerializeField] private TMP_InputField country;

        [SerializeField] private TMP_InputField platform;

        [SerializeField] private TMP_InputField xApiToken;

        [SerializeField] private TMP_InputField gameBackendHost;

        [SerializeField] private VideoPlayer backgroundPlayer;

        [SerializeField] private Image backgroundImage;

        /// <summary>
        /// Loads a configuration from the view.
        /// </summary>
        public void Load()
        {
            loadingScreen.SetActive(true);

            var config = new ConfigurationJSON()
            {
                host = host.text,
                expiresIn = expiresIn.text,
                userId = userId.text,
                appId = appId.text,
                deviceId = deviceId.text,
                countryCode = country.text,
                platform = platform.text,
                XApiToken = xApiToken.text,
                gameBackendHost = gameBackendHost.text,
            };

            var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            var overrideConfig = new TextAsset(configJson);

            TokenConfigurationJSONOverride = overrideConfig;

            // TODO: Load gem hunter scene
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SceneManager.LoadScene(DefaultInitScene);
                return;
            }

            using var binWriter = new BinaryWriter(File.Open(appDataConfigPath, FileMode.Create));

            //Do some simple encoding
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(configJson);
            binWriter.Write(Convert.ToBase64String(bytesToEncode));
            SceneManager.LoadScene(DefaultInitScene);
        }

        private IEnumerator WaitForBackgroundPlayer()
        {
            yield return new WaitUntil(() => backgroundPlayer.isPrepared && backgroundPlayer.isPlaying);

            loadingScreen.SetActive(false);
        }

        #region Unity Methods
        /// <inheritdoc/>
        private void Start()
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                loadingScreen.SetActive(true);
                backgroundImage.gameObject.SetActive(false);
                appDataConfigPath = Path.Combine(Application.persistentDataPath, AppDataConfigName);
                if (File.Exists(appDataConfigPath))
                {
                    using var reader = new BinaryReader(File.Open(appDataConfigPath, FileMode.Open));

                    //Decode and read
                    byte[] decodedBytes = Convert.FromBase64String(reader.ReadString());
                    jsonConfiguration = new(Encoding.UTF8.GetString(decodedBytes));
                }
                StartCoroutine(WaitForBackgroundPlayer());
            }
            else
            {
                backgroundPlayer.gameObject.SetActive(false);
                gameBackendHost.gameObject.SetActive(false);
                GameObject.FindGameObjectWithTag(HostLabelTag).SetActive(false);
            }

            var configJSON = JsonConvert.DeserializeObject<ConfigurationJSON>(jsonConfiguration.text);

            host.text = configJSON.host;
            expiresIn.text = configJSON.expiresIn;
            userId.text = configJSON.userId;
            appId.text = configJSON.appId;
            deviceId.text = configJSON.deviceId;
            country.text = configJSON.countryCode;
            platform.text = configJSON.platform;
            xApiToken.text = configJSON.XApiToken;
            gameBackendHost.text = configJSON.gameBackendHost;
        }
        #endregion Unity Methods
    }
}
