@echo off
set "psCommand=powershell -Command "$pword = read-host 'Enter api-key' -AsSecureString ; ^
    $BSTR=[System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pword); ^
        [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)""
for /f "usebackq delims=" %%p in (`%psCommand%`) do set api-key=%%p

dotnet nuget push bin\Release\xy.Db.SQLite64.1.0.4.nupkg --api-key %api-key% --source https://api.nuget.org/v3/index.json

pause