﻿# Stop on error, see http://redsymbol.net/articles/unofficial-bash-strict-mode/
$ErrorActionPreference = 'Stop'

### Args
if ($args.Count -ne 1) { write-host "Usage is $($MyInvocation.MyCommand.Path) version"; exit 1 }
# Version
[string]$version = $args[0]


write-host "### Is environment available"
if (!(test-path c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe)) { write-host 'msbuild not available !'; exit 1 }


write-host "### Paths"
$nugetSpecFileName = 'PMCG.Messaging.nuspec'
$rootDirectoryPath = (split-path ($MyInvocation.MyCommand.Path))
$solutionFilePath = join-path $rootDirectoryPath PMCG.Messaging.sln
$versionAttributeFilePath = join-path $rootDirectoryPath (join-path src SharedAssemblyInfo.cs)
$releaseDirectoryPath = join-path $rootDirectoryPath release
$nugetSpecFilePath = join-path $rootDirectoryPath $nugetSpecFileName
$nugetPackageFilePath = join-path $releaseDirectoryPath $nugetSpecFileName.Replace('.nuspec', '.' + $version + '.nupkg')


echo "### nuget ensure we have our dependencies"
nuget restore $solutionFilePath -verbosity detailed


write-host "### Compile"
# Change version attribute - Assembly and file
(get-content $versionAttributeFilePath) | % { $_ -replace 'Version\("[\d.]*"', "Version(`"$version`"" } | set-content $versionAttributeFilePath -encoding utf8
# Build
c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe $solutionFilePath /target:ReBuild /property:Configuration=Release
if ($LastExitCode -ne 0) { write-host 'Compile failure !'; exit 1 }
# Restore version attribute file
git checkout $versionAttributeFilePath


write-host "### Run tests"
# Ensure we have NUnit.Runners and get console app path
nuget install NUnit.Runners -outputdirectory packages -verbosity detailed
$nunitConsoleFilePath=(dir packages -r -include 'nunit-console.exe')[0].Fullname
# Test all release UT assemblies within the test directory
dir test -recurse -include *.UT.dll | ? { $_.FullName.IndexOf('bin\Release') -gt -1 } | % {
	& $nunitConsoleFilePath -framework:net-4.5 $_.FullName
	if ($LastExitCode -ne 0) { write-host 'Run tests failure !'; exit 1 }
}


write-host "### nuget pack"
# Ensure we have an empty release directory
if (test-path $releaseDirectoryPath) { rm -recurse -force $releaseDirectoryPath }
mkdir $releaseDirectoryPath | out-null
# nuget pack
nuget pack $nugetSpecFilePath -outputdirectory $releaseDirectoryPath -version $version -verbosity detailed


write-host "### Push instruction"
write-host "You can push the package just created to the default source (nuget.org) with the following command`n`t`$apiKey=key_from_nuget.org`n`tnuget push $nugetPackageFilePath -apikey `$apiKey -verbosity detailed"
