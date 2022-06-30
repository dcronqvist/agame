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
$startUpHelp = $args[1]

# Create new text file with the startup help text in it
New-Item -ItemType File -Path "./bin/Release/net6.0/win-x64/publish/STARTUP_HELP.txt" -Force
Set-Content -Path "./bin/Release/net6.0/win-x64/publish/STARTUP_HELP.txt" -Value $startUpHelp

$compress = @{
    Path             = "./bin/Release/net6.0/win-x64/publish/*"
    CompressionLevel = "Fastest"
    DestinationPath  = "./publish/$filename.zip"
    Force            = $true
}
Compress-Archive @compress

# Copy the zip to clipboard
Set-Clipboard -Path ./publish/$filename.zip