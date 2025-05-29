using Microsoft.Maui.Devices;

namespace PLMobile.Services
{
    public static class ApiConfiguration
    {
        // Production API URL - this should be your actual deployed server URL
        private static string ProductionUrl = "http://localhost:3000";  // During development we'll use localhost

        public static string GetApiUrl()
        {
            #if DEBUG
            // For development testing
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                if (DeviceInfo.Current.DeviceType == DeviceType.Virtual)
                {
                    // Android Emulator - use 10.0.2.2
                    return "http://10.0.2.2:3000";
                }
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                if (DeviceInfo.Current.DeviceType == DeviceType.Virtual)
                {
                    // iOS Simulator - use localhost
                    return "http://localhost:3000";
                }
            }
            
            // For physical devices in debug mode, use the production URL
            return ProductionUrl;
            #else
            // Release mode - always use production URL
            return ProductionUrl;
            #endif
        }

        public static string GetEndpoint(string path)
        {
            return $"{GetApiUrl().TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }
} 