[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $WorkingDirectory,
    [Parameter()]
    [string]
    $DockerCompose="docker-compose.yml",
    [Parameter()]
    [string]
    $ProjectName=$null,
    [Parameter()]
    [string]
    [ValidateSet("Up","Down","Restart")]
    $Operation="Up"
)

$oldDirectory = Get-Location
if(-not $(Test-Path $WorkingDirectory))
{
    $WorkingDirectory = Get-Location
}

Set-Location $WorkingDirectory

switch ($Operation) {
    "Up"{ 
        docker-compose --file "$DockerCompose" --project-name $ProjectName up -d}
    "Down"{ 
        docker-compose --file "$DockerCompose" --project-name $ProjectName down}
    "Restart"{ 
        docker-compose --file "$DockerCompose" --project-name $ProjectName restart}
    Default {}
}

Set-Location $oldDirectory