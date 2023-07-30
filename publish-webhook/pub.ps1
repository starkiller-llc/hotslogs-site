param (
  [switch]$SkipBuild, # Don't run MSBUILD
  [switch]$Dev, # Use dev site
  [string]$Dir, # Specifies the deployment directory. Can be A, B, C or maint
  [switch]$Docker # Don't look for IIS - good for running in the container
)

$pub = $true;

if (!$Docker) {
  $serverManager = Get-IISServerManager
  $binding = "^\*:443:hotslogs.com"
  if ($Dev.IsPresent) {
    $binding = "^\*:443:dev.hotslogs.com"
  }

  # Get the site according to the binding (prod or dev)
  $site = $serverManager.Sites | where-object { $_.bindings.bindinginformation -match $binding }
  if (!$site) {
    Write-Output "Can't find site with binding $binding"
    exit 1
  }
}

# Print usage if no arguments
if (!$SkipBuild.IsPresent -and !$Dir -and !$Dev.IsPresent) {
  Write-Output "Usage: pub [-SkipBuild] [-Dev] [-Dir <A|B|C|dev|maint>"
  if (!$Docker) {
    Write-Output "Current site ($binding) served from $($site.Applications.VirtualDirectories.PhysicalPath)"
  }
  exit 1
}

# Check validity of -Dir argument
if ($Dir -and !("a", "b", "c", "dev", "maint" -eq $Dir)) {
  Write-Output $Dir
  Write-Output "-Dir must be a, b, c, maint or dev"
  exit 1
}

if (!$Docker) {
  # Print out current served directory
  Write-Output "Current site ($binding) served from $($site.Applications.VirtualDirectories.PhysicalPath)"
}

# If no -Dir argument, just exit now after showing the information
if (!$Dir) {
  exit 0
}

if ($Dir -eq "maint") {
  if (!$Docker) {
    # If maintenance was requested simply set the directory to C:\HOTSLogs-Maintenance
    if ($pub) {
      $site.Applications[0].VirtualDirectories[0].PhysicalPath = "C:\HOTSLogs-Maintenance"
      $serverManager.CommitChanges()
      Write-Output "Published to C:\HOTSLogs-Maintenance"
    }
    else {
      Write-Output "Would publish to maintenance"
    }
  }
}
elseif ($Dir -eq "dev") {
  if (!$Docker) {
    # If dev was requested simply set the directory to D:\StarkillerLLC\HOTSLogs\Heroes.WebApplication
    $devdir = "D:\StarkillerLLC\HOTSLogs\Heroes.WebApplication"
    if ($pub) {
      $site.Applications[0].VirtualDirectories[0].PhysicalPath = $devdir
      $serverManager.CommitChanges()
      Write-Output "Published to $devdir"
    }
    else {
      Write-Output "Would publish to $devdir"
    }
  }
}
else {
  # A, b or c was requested
  $pubprof = "HOTSLogs-Main-" + $Dir
  $path = "C:\HOTSLogs-Main-" + $Dir

  # If -SkipBuild was not specified, run msbuild to build and publish the project
  if (!$SkipBuild) {

    if ($Docker) {
      & NuGet.exe restore ..\HeroesReplays.sln
      & dotnet restore ..\HeroesReplays.sln
      $projdir = "..\Heroes.WebApplication\HeroesWebApplication.csproj"
      $msbuild = "msbuild"
    }
    else {
      # See if we are running from within a source directory.
      # If not, take source from D:\StarkillerLLC\HOTSLogs
      if (Test-Path "..\Heroes.WebApplication\HeroesWebApplication.csproj") {
        $projdir = "..\Heroes.WebApplication\HeroesWebApplication.csproj"
      }
      else {
        $projdir = "D:\StarkillerLLC\HOTSLogs\Heroes.WebApplication\HeroesWebApplication.csproj"
      }
  
      # Use vswhere.exe to find msbuild.exe
      $msbuild = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
    }
  
    & $msbuild $projdir -p:DeployOnBuild=true -p:PublishProfile=$pubprof -p:Configuration=Release -v:m
    if ($LASTEXITCODE) {
      Write-Output "Build failed - not publishing."
      exit 1
    }
  }

  # Set serve directory to publish target directory
  if ($pub) {
    if (!$Docker) {
      $site.Applications[0].VirtualDirectories[0].PhysicalPath = $path
      $serverManager.CommitChanges()
    }
    Write-Output "Published to $path"
  }
  else {
    Write-Output "Would publish to $path"
  }

  if (!$Docker) {
    # Send the discord notification
    & npm start
  }
}
