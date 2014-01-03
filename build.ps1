# Push current location
pushd;

# Paths
$rootDirectoryPath = split-path $MyInvocation.MyCommand.Path;
$outputDirectoryPath = join-path $rootDirectoryPath 'bin';
$outputDllFilePath = join-path $outputDirectoryPath 'PMCG.Messaging.Client.dll';
$ilmergeExePath = join-path $rootDirectoryPath 'lib\ilmerge\ilmerge.exe';

# Change directories
pushd;
cd $rootDirectoryPath;

# Build
c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe src\PMCG.Messaging.Client\PMCG.Messaging.Client.csproj /target:ReBuild /property:Configuration=Release;

# Prepare output directory
if (test-path $outputDirectoryPath){ rm $outputDirectoryPath\*; } else { mkdir $outputDirectoryPath | out-null; }
$version = git log -1 --pretty=format:%H;
"Built @ {0}`r`nBy {1}`r`nVersion {2}" -f (get-date), $env:UserName, $version > (join-path $outputDirectoryPath 'BuildInfo.txt');

# Merge RabbitMQ dependency - only RabbitMQ as I expect other dependencies will be used by clients independently of this project
& $ilmergeExePath `
	/target:library `
	/v4 `
	/out:$outputDllFilePath `
	src\PMCG.Messaging.Client\bin\release\PMCG.Messaging.Client.dll `
	src\PMCG.Messaging.Client\bin\release\RabbitMQ.Client.dll;

# Copy all other content to output directory
dir D:\myoss\PMCG.Messaging\src\PMCG.Messaging.Client\bin\Release | ? { $_.Name -notlike 'RabbitMQ.*' -and $_.Name -notlike 'PMCG.Messaging.Client.*' } | % { cp $_.Fullname $outputDirectoryPath }

# Result
write-host "`n`n`n*****`nBinaries are available @ $outputDirectoryPath`n*****`n"

# Pop back to original location
popd;
