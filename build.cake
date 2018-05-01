#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.Git
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
const string ProjectName = "Furysoft.Serializers.Versioning";

var buildDir = Directory("./src/Component/" + ProjectName + "/bin") + Directory(configuration);

var sln = "./src/" + ProjectName + ".sln";
const string Project = "./src/Component/" + ProjectName + "/" + ProjectName + ".csproj";
const string TestProject = "./src/Tests/" + ProjectName + ".Tests/" + ProjectName + ".Tests.csproj";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(sln);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild(sln,
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                ArgumentCustomization = args => args.Append("--no-restore"),
            });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest(TestProject,
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true,
                    ArgumentCustomization = args => args.Append("--no-restore"),
                });
});

Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    DotNetCorePack(Project, new DotNetCorePackSettings
     {
         Configuration = configuration,
         NoRestore = true,
         NoBuild = true
    });
});

Task("Push-To-NuGet")
    .IsDependentOn("Package")
    .Does(() =>
{
var nugetKey=EnvironmentVariable("NugetApi");

      var settings = new DotNetCoreNuGetPushSettings
     {
         ApiKey = nugetKey,
         Source = "https://api.nuget.org/v3/index.json"
     };

     DotNetCoreNuGetPush("**/" + ProjectName + ".*.nupkg", settings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Push-To-NuGet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
