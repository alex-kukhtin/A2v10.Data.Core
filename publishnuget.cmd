del /q A2v10.Data.Interfaces\bin\Release\*.nupkg
del /q A2v10.Data.Interfaces\bin\Release\*.snupkg
del /q A2v10.Data.Providers\bin\Release\*.nupkg
del /q A2v10.Data.Providers\bin\Release\*.snupkg
del /q A2v10.Data\bin\Release\*.nupkg
del /q A2v10.Data\bin\Release\*.snupkg

dotnet build -c Release

del /q ..\NuGet.local\*.*

copy A2v10.Data.Interfaces\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Data.Interfaces\bin\Release\*.snupkg ..\NuGet.local

copy A2v10.Data.Providers\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Data.Providers\bin\Release\*.snupkg ..\NuGet.local

copy A2v10.Data\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Data\bin\Release\*.snupkg ..\NuGet.local

pause