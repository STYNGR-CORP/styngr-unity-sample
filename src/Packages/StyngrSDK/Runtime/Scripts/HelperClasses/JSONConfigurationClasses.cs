using System;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    [Serializable]
    public class BodyConfiguration
    {
        public string expiresIn;
        public string userId;
        public string appId;
        public string deviceId;
        public string countryCode;
        public string platform;
    }

    [Serializable]
    public class ConfigurationJSON : BodyConfiguration
    {
        public string host;
        public string XApiToken;
        public string gameBackendHost;

        public readonly string billingCountry = "US";

        public BodyConfiguration GetBody() =>
            new()
            {
                expiresIn = expiresIn,
                userId = userId,
                appId = appId,
                deviceId = deviceId,
                countryCode = countryCode,
                platform = platform
            };
    }
}
