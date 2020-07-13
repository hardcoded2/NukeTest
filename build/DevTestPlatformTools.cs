using System.Runtime.InteropServices;

namespace DevTests
{
    public class DevTestPlatformTools
    {
        bool RunningOnWindows()
        {
            return RunningOnPlatform(OSPlatform.Windows);
        }

        bool RunningOnMac()
        {
            return RunningOnPlatform(OSPlatform.OSX);
        }
        bool RunningOnPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }
    }
}
