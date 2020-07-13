using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    [LocalExecutable("./tools/protoc/protoc.exe")]
    readonly Tool Protoc;

    AbsolutePath SourceDirectory => RootDirectory / "Inventory";
    AbsolutePath TestsDirectory => RootDirectory / "Inventory.Tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target CleanGeneratedProtocs => _ => _
        .Executes(() =>
        {
            AbsolutePath ProtogenOutputDir = RootDirectory / "ExampleCustomProtoBufStructure/gen";

            var protogenGeneratedCsFiles = new DirectoryInfo(ProtogenOutputDir).EnumerateFiles("*.cs");
            protogenGeneratedCsFiles.ForEach(fileInfo => fileInfo.Delete());
            
        });
    Target DevTestTools => _ => _
        .Executes(() =>
        {
            ManualToolresolvingMeandering();
        });
    void ManualToolresolvingMeandering()
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
        try
        {
           Tool doesNotExist = ToolResolver.GetPathTool("DOES NOT EXIST");
        }catch(Exception e){}
        try
        {
            ControlFlow.Fail("TEST same as trying to get non existent tool with tool resolver");
        }catch(Exception e){}
    }

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
    Target BuildProtosToDLL => _ => _
        .Executes(() =>
        {
            var genProtosFromThisDir = RootDirectory / "ExampleCustomProtoBufStructure";
            Logger.Normal(genProtosFromThisDir);
            Protoc.Invoke(
                "--proto_path=protos --csharp_out=gen protos/*.proto",genProtosFromThisDir);
        });
    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

}
