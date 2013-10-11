# Output path
$outputDirectoryPath = join-path (split-path $MyInvocation.MyCommand.Path) 'bin'

# Build
c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe src\PMCG.Messaging.Client\PMCG.Messaging.Client.csproj `
	/target:ReBuild `
	/property:Configuration=Release `
	/property:OutputPath=$outputDirectoryPath;

# Result
write-host "`n`n`n*****`nBinaries are available @ $outputDirectoryPath`n*****`n"