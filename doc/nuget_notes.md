# Links
- http://docs.nuget.org/create/creating-and-publishing-a-package
- http://docs.nuget.org/
- https://github.com/michael-wolfenden/Polly/blob/master/build.ps1
- https://github.com/net-commons/common-logging/blob/master/src/Common.Logging.Log4Net1213/Common.Logging.Log4Net1213.nuspec
- http://damieng.com/blog/2014/01/08/simple-steps-for-publishing-your-nuget-package
- 

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

# Create the nuget spec file
```bash
cd nuginv
nuget spec PMCG.Messaging
vim PMCG.Messaging.nuspec
```

# Create nuget package
```bash
nuget pack PMCG.Messaging.nuspec -outputdirectory /tmp -version '1.0.1' -verbosity detailed
```

# Push nuget package - Im using the default source\registry which is nuget.org - I have account with an api key
```bash
api_key=1.......fillin......	
nuget push /tmp/PMCG.Messaging.1.0.1.nupkg
```
