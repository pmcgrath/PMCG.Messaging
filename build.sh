#!/usr/bin/env bash
### Args
[ $# -ne 1 ] && echo "Usage is $0 version" && exit 1
# Version
version=$1


echo "### Is environment available"
[ -z $(which mono) ] && (echo 'mono not available !' && exit 1)
[ -z $(which xbuild) ] && (echo 'xbuild not available !' && exit 1)
[ -z $(which nuget) ] && (echo 'nuget not available !' && exit 1)


echo "### Paths"
root_directory_path=$(dirname $(readlink -f $0))
solution_file_path=$root_directory_path/src/PMCG.Messaging.sln
version_attribute_file_path=$root_directory_path/src/SharedAssemblyInfo.cs
nuget_spec_file_path=$root_directory_path/PMCG.Messaging.Client.nuspec


echo "### Compile"
# Change version attribute - Assembly and file
sed -i "s/Version(\"[0-9.]*/Version(\"$version/g" $version_attribute_file_path
# Build
xbuild $solution_file_path /target:ReBuild /property:Configuration=Release
[ $? != 0 ] && echo "Compile failure !" && exit 1
# Restore version attribute file
git checkout $version_attribute_file_path


echo "### Run tests - Test all release UT assemblies within the test directory"
find ./test -name '*.UT.dll' | grep '/bin/Release/' | xargs mono ./lib/NUnit/bin/nunit-console.exe
[ $? != 0 ] && echo "Run tests failure !" && exit 1


echo "### nuget pack"
# Switch to unix style path separators
sed -i 's#\\#/#g' $nuget_spec_file_path
# Ensure we have an empty release directory
release_directory_path=$root_directory_path/release
[ -d $release_directory_path ] && rm $release_directory_path -r
mkdir $release_directory_path
# nuget pack
nuget pack $nuget_spec_file_path -outputdirectory $release_directory_path -version $version -verbosity detailed
# Restore nuget spec file - windows separators
git checkout $nuget_spec_file_path
