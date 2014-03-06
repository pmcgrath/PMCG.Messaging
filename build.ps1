# Push current location
pushd;

# Paths
$rootDirectoryPath = split-path $MyInvocation.MyCommand.Path;
$outputDirectoryPath = join-path $rootDirectoryPath 'bin';

# Change directories
pushd;
cd $rootDirectoryPath;

# Build
c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe src\PMCG.Messaging.Client\PMCG.Messaging.Client.csproj /target:ReBuild /property:Configuration=Release;

# Prepare output directory
if (test-path $outputDirectoryPath){ rm $outputDirectoryPath\*; } else { mkdir $outputDirectoryPath | out-null; }
$version = git log -1 --pretty=format:%H;
"Built @ {0}`r`nBy {1}`r`nVersion {2}" -f (get-date), $env:UserName, $version > (join-path $outputDirectoryPath 'BuildInfo.txt');

# Copy content to output directory
dir D:\myoss\PMCG.Messaging\src\PMCG.Messaging.Client\bin\Release | % { cp $_.Fullname $outputDirectoryPath }

# Result
write-host "`n`n`n*****`nBinaries are available @ $outputDirectoryPath`n*****`n"

# Pop back to original location
popd;
