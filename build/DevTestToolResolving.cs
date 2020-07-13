using System;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;

namespace DevTests
{
    public class DevTestToolResolving
    {
        public static void ManualToolresolvingMeandering()
            {
                string TryGetPathExe(string exe)
                {
                    try
                    {
                        return ToolPathResolver.GetPathExecutable(exe);
                    }
                    catch
                    {
                    }
        
                    return string.Empty;
                }
                string TryGetPathToolExe(string exe)
                {
                    try
                    {
                        return ToolPathResolver.TryGetEnvironmentExecutable(exe);
                    }
                    catch
                    {
                    }
        
                    return string.Empty;
                }
                //ToolResolver.GetPathTool(exe)
                    
                //ToolResolver.GetPathTool("appveyor");
                //Tool myTool = ToolResolver.GetPathTool(exe)
                void TryAndGetPathEXEAndPrint(string exe)
                {
                    Logger.Normal($"Trying to get exe {exe}: '{TryGetPathExe(exe)}'");
                }
                var executables = new[]{"DOES NOT EXIST","powershell","bash"};
                //ToolPathResolver.GetPathExecutable - get something on the path 
                //vs TryGetEnvironmentExecutable             //ToolPathResolver.TryGetEnvironmentExecutable -- this will get an exe defined by an enviornment variable
                //vs ToolResolver.GetPathTool(exe) - get a tool in the user's path based on 
                executables.ForEach(TryAndGetPathEXEAndPrint);    
                Logger.Normal($"Comspec is set up by something on windows systems as a standard exe tool, so here is the path {TryGetPathToolExe("ComSpec")}");
                Tool git = ToolResolver.GetPathTool("git");
#pragma warning disable 168 
                //just showing that ControlFlow.Fail throws an "Exception" object that is not sub-typed
                try
                {
                   Tool doesNotExist = ToolResolver.GetPathTool("DOES NOT EXIST");
                }catch(Exception e){}
                try
                {
                    ControlFlow.Fail("TEST same as trying to get non existent tool with tool resolver");
                }catch(Exception e){}
#pragma warning restore 168

            }
        
    }
}
