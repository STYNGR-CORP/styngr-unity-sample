namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public static class Common
    {
        public static class STATUS
        {
            public static readonly string AVAILABLE = "AVAILABLE";
            public static readonly string UNAVAILABLE = "UNAVAILABLE";
            public static readonly string PURCHASED = "PURCHASED";

            public static bool Compare(string A, string B)
            {
                if (A == null && B == null)
                {
                    return true;
                }

                if (A == null || B == null)
                {
                    return false;
                }

                return A.ToLower() == B.ToLower();
            }
        }
    }
}
