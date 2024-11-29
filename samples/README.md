# Samples

## How to build and run a sample project.

1. Publish a sample project on Linux

Due to the version of glibc, I recommend building with Ubuntu 20.04.

```
cd <SampleProjectName>

# Native AOT project
dotnet publish -r linux-x64

# Non-Native AOT project
dotnet publish -r linux-x64 --self-contained
```

2. Run httpd (Apache) in Docker on Windows
```
cd <SampleProjectName>
docker run --rm -p 8080:80 -v %cd%\..\Docker\httpd.conf:/usr/local/apache2/conf/httpd.conf -v %cd%\bin\Release\net9.0\linux-x64\publish:/usr/local/apache2/cgi-bin/ httpd:2.4
```

3. Finally, Open `http://localhost:8080/cgi-bin/<SampleProjectName>` in your browser.