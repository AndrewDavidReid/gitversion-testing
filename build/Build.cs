using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [GitRepository] readonly GitRepository GitRepository;
  [GitVersion] readonly GitVersion GitVersion;

  [Solution] readonly Solution Solution;

  AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

  Target Debug => _ => _
    .Executes(() =>
    {
      Logger.Info($"AssemblySemVer: {GitVersion.AssemblySemVer}");
      Logger.Info($"AssemblySemFileVer: {GitVersion.AssemblySemFileVer}");
    });

  Target Clean => _ => _
    .Executes(() =>
    {
      EnsureCleanDirectory(ArtifactsDirectory);
    });

  Target Restore => _ => _
    .Executes(() =>
    {
      DotNetRestore(s => s
        .SetProjectFile(Solution));
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

  /// Support plugins are available for:
  /// - JetBrains ReSharper        https://nuke.build/resharper
  /// - JetBrains Rider            https://nuke.build/rider
  /// - Microsoft VisualStudio     https://nuke.build/visualstudio
  /// - Microsoft VSCode           https://nuke.build/vscode
  public static int Main() => Execute<Build>(x => x.Compile);
}
