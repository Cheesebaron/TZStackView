#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"

var sln = new FilePath("TZStackView.sln");
var project = new FilePath("TZStackView/TZStackView.csproj");
var binDir = new DirectoryPath("TZStackView/bin/Release");
var nuspec = new FilePath("TZStackView.nuspec");
var outputDir = new DirectoryPath("artifacts");
var target = Argument("target", "Default");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
	CleanDirectories(outputDir.FullPath);
});

GitVersion versionInfo = null;
Task("Version").Does(() => {
	GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.BuildServer
	});

	versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
	Information("VI:\t{0}", versionInfo.FullSemVer);
});

Task("Restore").Does(() => {
	NuGetRestore(sln);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.Does(() =>  {
	
	DotNetBuild(project, 
		settings => settings.SetConfiguration("Release")
							.WithTarget("Build")
	);
});

Task("Package")
	.IsDependentOn("Build")
	.Does(() => {
	if (IsRunningOnWindows()) //pdbstr.exe and costura are not xplat currently
		GitLink(sln.GetDirectory(), new GitLinkSettings {
			RepositoryUrl = "https://github.com/Cheesebaron/TZStackView",
			ArgumentCustomization = args => args.Append("-ignore Sample")
		});

	EnsureDirectoryExists(outputDir);

	var dllDir = binDir + "/TZStackView.*";

	Information("Dll Dir: {0}", dllDir);

	var nugetContent = new List<NuSpecContent>();
	foreach(var dll in GetFiles(dllDir)){
	 	Information("File: {0}", dll.ToString());
		nugetContent.Add(new NuSpecContent {
			Target = "lib/Xamarin.iOS10",
			Source = dll.ToString()
		});
	}

	Information("File Count {0}", nugetContent.Count);

	NuGetPack(nuspec, new NuGetPackSettings {
		Authors = new [] { "Tomasz Cielecki" },
		Owners = new [] { "Tomasz Cielecki" },
		IconUrl = new Uri("http://i.imgur.com/V3983YY.png"),
		ProjectUrl = new Uri("https://github.com/Cheesebaron/TZStackView"),
		LicenseUrl = new Uri("https://github.com/Cheesebaron/TZStackView/blob/master/LICENSE"),
		Copyright = "Copyright (c) Tomasz Cielecki",
		RequireLicenseAcceptance = false,
		Tags = new [] {"xamarin", "monotouch", "ios", "stackview"},
		Version = versionInfo.NuGetVersion,
		Symbols = false,
		NoPackageAnalysis = true,
		OutputDirectory = outputDir,
		Verbosity = NuGetVerbosity.Detailed,
		Files = nugetContent,
		BasePath = "/."
	});
});

Task("UploadAppVeyorArtifact")
	.IsDependentOn("Package")
	.WithCriteria(() => !isPullRequest)
	.WithCriteria(() => isRunningOnAppVeyor)
	.Does(() => {

	Information("Artifacts Dir: {0}", outputDir.FullPath);

	foreach(var file in GetFiles(outputDir.FullPath + "/*")) {
		Information("Uploading {0}", file.FullPath);
		AppVeyor.UploadArtifact(file.FullPath);
	}
});

Task("Default").IsDependentOn("UploadAppVeyorArtifact").Does(() => {});

RunTarget(target);