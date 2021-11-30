[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("consul-server","app","consul-app")]
    [string]
    $BuildTarget
)

$originalDirectory = Get-Location

if([string]::IsNullOrWhiteSpace($DockerFile))
{
    $DockerFile = "Dockerfile"
}

$Directories =@()
if($BuildTarget -eq "All")
{
    $Directories = @("consul-server","app","consul-app")
}else{
    $Directories = @($BuildTarget.ToLower())
}


foreach ($directory in $Directories) {
    $WorkingDirectory = "src\containers\$directory"
    Set-Location $WorkingDirectory
    docker build . --tag isago.ch/gbo/ocelot-dc-lab/$directory
    Set-Location $originalDirectory
}

Set-Location $originalDirectory