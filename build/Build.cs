using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DevTests;
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

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution("Backend.sln")] readonly Solution BackendSolution;

    

    AbsolutePath ExampleProtobufStructureDir => RootDirectory / "ExampleCustomProtoBufStructure";

    [Solution("ExampleCustomProtoBufStructure/ExampleCustomProtoBufStructure.csproj")]
    readonly Solution ProtobufCSDLLSolution;

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;
    [LocalExecutable("./tools/protoc/protoc.exe")] readonly Tool Protoc;

    AbsolutePath SourceDirectory => RootDirectory / "Inventory";
    AbsolutePath TestsDirectory => RootDirectory / "Inventory.Tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target DevTests => _ => _
        .Executes(() =>
        {
            var platformDevTests = new DevTestPlatformTools();
            Logger.Normal($"Running on platform {platformDevTests.GetPlatform()} and is this windows {platformDevTests.IsWindows()} is this mac {platformDevTests.IsMac()}");
            
            Logger.Normal($"static project path  {ProtobufCSDLLSolution}");
            Solution ProtobufCSDLLSolutionDynamic = DevTestDynamicSolution.SolutionDynamic((RootDirectory /
                                                                                             "ExampleCustomProtoBufStructure/ExampleCustomProtoBufStructure.csproj"
                ));
            Logger.Normal($"dynamic project path {ProtobufCSDLLSolutionDynamic}");
            ControlFlow.Assert(ProtobufCSDLLSolution == ProtobufCSDLLSolutionDynamic,"Dynamically generated protobuf cs project doesn't match");
            DevTestToolResolving.ManualToolresolvingMeandering();
        });
    Target Clean => _ => _
        .Before(Restore)
        .DependsOn(CleanGeneratedProtocs)
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
                .SetProjectFile(BackendSolution));
            DotNetRestore(s => s
                .SetProjectFile(ProtobufCSDLLSolution)); //not sure if this should be in this target. or if "restore" should exist in the way it does here
        });

    Target CleanGeneratedProtocs => _ => _
        .Executes(() =>
        {
            AbsolutePath ProtogenOutputDir = RootDirectory / "ExampleCustomProtoBufStructure/gen";

            var protogenGeneratedCsFiles = new DirectoryInfo(ProtogenOutputDir).EnumerateFiles("*.cs");
            protogenGeneratedCsFiles.ForEach(fileInfo => fileInfo.Delete());
            ExampleProtobufStructureDir.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target BuildProtosToDLL => _ => _
        .DependsOn(CleanGeneratedProtocs)
        .Executes(() =>
        {
            var genProtosFromThisDir = RootDirectory / "ExampleCustomProtoBufStructure";
            Logger.Normal($"where we are generating protobufs from {genProtosFromThisDir}");
            Protoc.Invoke(
                "--proto_path=protos --csharp_out=gen protos/*.proto",genProtosFromThisDir);
            DotNetRestore(s => s
                .SetProjectFile(ProtobufCSDLLSolution));
            DotNetBuild(s => s
                .SetProjectFile(ProtobufCSDLLSolution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
            //output files -- there has to be a better way???
            var generatedFiles = (genProtosFromThisDir / "bin").GlobFiles("**"); //.GlobDirectories("**/bin", "**/obj");
            Logger.Normal($"Files: {string.Join(",",generatedFiles) }");
            //example debug configuration: Files: E:\dev\NukeTest\ExampleCustomProtoBufStructure\bin\Debug\netcoreapp3.1\ExampleCustomProtoBufStructure.deps.json,E:\dev\NukeTest\ExampleCustomProtoBufStructure\bin\Debug\netcoreapp3.1\ExampleCustomProtoBufStructure.dll,E:\dev\NukeTest\ExampleCustomProtoBufStructure\bin\Debug\netcoreapp3.1\ExampleCustomProtoBufStructure.pdb
        });
    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(BuildProtosToDLL)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(BackendSolution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });
    Target DevTestTools => _ => _
        .Executes(DevTestToolResolving.ManualToolresolvingMeandering);
}
