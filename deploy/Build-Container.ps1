[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Client","Server","App","All")]
    [string]
    $BuildTarget
)

$originalDirectory = Get-Location

if(-not $(Test-Path $WorkingDirectory))
{
    $WorkingDirectory = Get-Location
}
if([string]::IsNullOrWhiteSpace($DockerFile))
{
    $DockerFile = "Dockerfile"
}

$Directories =@()
if($BuildTarget -eq "All")
{
    $Directories = @("client","server","app")
}else{
    $Directories.Clear()
    $Directories.Add($BuildTarget.ToLower())
}


foreach ($directory in $Directories) {
    $WorkingDirectory = "..\src\containers\consul-$directory"
    Set-Location $WorkingDirectory
    docker build . --tag isago.ch/gbo/ocelot-dc-lab/consul-$directory
}

Set-Location $originalDirectory