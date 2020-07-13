using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DevTests
{
    public class DevTestPlatformTools
    {
        public bool IsWindows()
        {
            return RunningOnPlatform(OSPlatform.Windows);
        }

        public bool IsMac()
        {
            return RunningOnPlatform(OSPlatform.OSX);
        }
        public bool RunningOnPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }

        public OSPlatform GetPlatform()
        {
            foreach (var platform in System.Enum.GetValues(typeof(OSPlatform)).Cast<OSPlatform>())
            {
                if (RuntimeInformation.IsOSPlatform(platform)) return platform;
            }
            throw new Exception("Unhandled platform!");
        }
    }
}
