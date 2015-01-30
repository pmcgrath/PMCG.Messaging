### Args
if ($args.Count -ne 1) { write-host "Usage is $($MyInvocation.MyCommand.Path) version"; exit 1 }
# Version
$version = $args[0]


write-host "### Is environment available"
if (!(test-path c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe)) { write-host 'msbuild not available !'; exit 1 }


write-host "### Paths"
$rootDirectoryPath = (split-path ($MyInvocation.MyCommand.Path))
$solutionFilePath = join-path $rootDirectoryPath (join-path src PMCG.Messaging.sln)
$versionAttributeFilePath = join-path $rootDirectoryPath (join-path src SharedAssemblyInfo.cs)
$nugetSpecFilePath = join-path $rootDirectoryPath PMCG.Messaging.Client.nuspec


write-host "### Compile"
# Change version attribute - Assembly and file
(get-content $versionAttributeFilePath) | % { $_ -replace 'Version\("[\d.]*"', "Version(`"$version`"" } | set-content $versionAttributeFilePath -encoding utf8
# Build
c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe $solutionFilePath /target:ReBuild /property:Configuration=Release
if ($LastExitCode -ne 0) { write-host 'Compile failure !'; exit 1 }	
# Restore version attribute file
git checkout $versionAttributeFilePath


write-host "### Run tests - Test all release UT assemblies within the test directory"
dir test -recurse -include *.UT.dll | ? { $_.FullName.IndexOf('bin\Release') -gt -1 } | % { 
	.\lib\NUnit\bin\nunit-console.exe $_.FullName
	if ($LastExitCode -ne 0) { write-host 'Run tests failure !'; exit 1 }
}


write-host "### nuget pack"
# Ensure we have an empty release directory
$releaseDirectoryPath = join-path $rootDirectoryPath release
if (test-path $releaseDirectoryPath) { rm -recurse -force $releaseDirectoryPath }
mkdir $releaseDirectoryPath | out-null
# nuget pack
nuget pack $nugetSpecFilePath -outputdirectory $releaseDirectoryPath -version $version -verbosity detailed
