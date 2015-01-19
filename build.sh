#!/usr/bin/env bash 
# Have kept the same as the powershell file

# Ensure runtime available
[ -z $(which xbuild)] && (echo 'mono not available ?' && exit 1)

# Push current location
pushd .

# Paths
rootDirectoryPath=$(dirname $(readlink -f $0))
outputDirectoryPath=$rootDirectoryPath/bin

# Change directories
cd $rootDirectoryPath

# Build
# Could just build the client csproj but want to make sure all projects are good
xbuild ./src/PMCG.Messaging.sln /target:ReBuild /property:Configuration=Release

# Prepare output directory
[ -d $outputDirectoryPath ] && rm $outputDirectoryPath -r
mkdir $outputDirectoryPath
version=$(git log -1 --pretty=format:%H)
echo -e "Built @ $(date)\nBy $USER\nVersion $version" > $outputDirectoryPath/BuildInfo.txt

# Copy content to output directory
cp ./src/PMCG.Messaging.Client/bin/Release/* $outputDirectoryPath

# Result
echo -e "\n\n\n*****\nBinaries are available @ $outputDirectoryPath\n*****\n"

# Pop back to original location
popd
