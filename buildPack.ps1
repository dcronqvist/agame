# Remove bin directory to ensure clean
Remove-Item .\bin -Force -Recurse -ErrorAction SilentlyContinue

# Create publish pack
dotnet publish --configuration Release --use-current-runtime --self-contained

# Copy assets to publish folder
Copy-Item -r ./mods ./bin/Release/net6.0/win-x64/publish/ -Force

# Copy settings.json
Copy-Item -r ./settings.json ./bin/Release/net6.0/win-x64/publish/ -Force

# Copy worlds
Copy-Item -r ./worlds ./bin/Release/net6.0/win-x64/publish/ -Force

# Copy libs to publish folder
Copy-Item -r ./libs/win/* ./bin/Release/net6.0/win-x64/publish/ -Force

# Create folder publish in root dir
New-Item -ItemType Directory -Force ./publish

$filename = $args[0]
$startUpCommand = $args[1]

# Create a new batch file with the startup command in it
New-Item -ItemType File -Path "./bin/Release/net6.0/win-x64/publish/STARTUP.bat" -Force
Set-Content -Path "./bin/Release/net6.0/win-x64/publish/STARTUP.bat" -Value $startUpCommand

$compress = @{
    Path             = "./bin/Release/net6.0/win-x64/publish/*"
    CompressionLevel = "Fastest"
    DestinationPath  = "./publish/$filename.zip"
    Force            = $true
}
Compress-Archive @compress

# Copy the zip to clipboard
Set-Clipboard -Path ./publish/$filename.zip