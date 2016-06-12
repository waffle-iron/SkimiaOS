xbuild /p:Configuration=Release SkimiaOS.sln /p:TargetFrameworkVersion="v4.5" /p:DebugSymbols=False
mkdir SkimiaOS.CLI.ServerConsole/bin/Release/core/
cp -R Plugins/SkimiaOS.Plugins.BasePlugin/bin/Release/* SkimiaOS.CLI.ServerConsole/bin/Release/core/
