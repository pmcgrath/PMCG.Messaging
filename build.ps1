# Push current location
pushd;

# Ensure runtime available
if (! test-path c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe){ write-host 'No compiler ?'; exit 1; }

# Paths
$rootDirectoryPath = split-path $MyInvocation.MyCommand.Path;
$outputDirectoryPath = join-path $rootDirectoryPath 'bin';

# Change directories
cd $rootDirectoryPath;

# Build
# Could just build the client csproj but want to make sure all projects are good
c:\windows\microsoft.net\framework64\v4.0.30319\msbuild.exe src\PMCG.Messaging.sln /target:ReBuild /property:Configuration=Release;

# Prepare output directory
if (test-path $outputDirectoryPath){ rm $outputDirectoryPath\*; } else { mkdir $outputDirectoryPath | out-null; }
$version = git log -1 --pretty=format:%H;
"Built @ {0}`r`nBy {1}`r`nVersion {2}" -f (get-date), $env:UserName, $version > (join-path $outputDirectoryPath 'BuildInfo.txt');

# Copy content to output directory
dir src\PMCG.Messaging.Client\bin\Release | % { cp $_.Fullname $outputDirectoryPath }

# Result
write-host "`n`n`n*****`nBinaries are available @ $outputDirectoryPath`n*****`n"

# Pop back to original location
popd;
