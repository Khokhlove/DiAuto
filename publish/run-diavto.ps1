$ErrorActionPreference = "Continue"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$logDirectory = Join-Path $root "logs"
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$logFile = Join-Path $logDirectory ("app-{0}.log" -f (Get-Date -Format "yyyyMMdd"))

# Не запускаем второй экземпляр сервера при повторном входе в систему.
$mutex = New-Object System.Threading.Mutex($false, "Global\DiAvtoWebServer")
if (-not $mutex.WaitOne(0)) {
    exit
}

Set-Location $root
$env:ASPNETCORE_ENVIRONMENT = "Production"

while ($true) {
    $started = Get-Date
    "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting DiAvto.Web..." | Out-File -FilePath $logFile -Append -Encoding UTF8

    & dotnet (Join-Path $root "DiAvto.Web.dll") *>> $logFile

    "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] DiAvto.Web stopped after $([math]::Round(((Get-Date) - $started).TotalSeconds)) s. Restarting in 5 s." |
        Out-File -FilePath $logFile -Append -Encoding UTF8

    Start-Sleep -Seconds 5
}
