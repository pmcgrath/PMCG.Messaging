# Links
- http://docs.nuget.org/create/creating-and-publishing-a-package
- http://docs.nuget.org/
- https://github.com/michael-wolfenden/Polly/blob/master/build.ps1
- https://github.com/net-commons/common-logging/blob/master/src/Common.Logging.Log4Net1213/Common.Logging.Log4Net1213.nuspec


# mono in docker notes
- Issue with using nuget on mono v3.12.0 docker container
 * http://stackoverflow.com/questions/25935382/nuget-packages-for-visual-studio-2012-will-not-restore-in-monodevelop-5-0-1
   * Had to add missing trusted root certificates with the following command
```bash
mozroots --import --sync
```
- To see existing nuget package content
```bash
cd /tmp
nuget install ServiceStack.RabbitMq
pkg_name=$(ls -t | grep ServiceStack.RabbitMq | head -n 1)
cd $pkg_name
unzip ${pkg_name}.nupkg -d source
find ./source -ls
```

# To create the nuget spec
```bash
cd nuginv
nuget spec PMCG.Messaging.Client
edit the spec
	.

mkdir lib
# Build content and place binaries in lib

nuget pack PMCG.Messaging.Client.nuspec -o /tmp/ -version '1.0.1'
```

