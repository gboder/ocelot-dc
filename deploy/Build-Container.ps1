[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $WorkingDirectory,
    # Parameter help description
    [Parameter()]
    [string]
    $DockerFile
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

Set-Location $WorkingDirectory

docker build . --tag isago.ch/gbo/ocelot-dc-lab/consul-server

Set-Location $originalDirectory