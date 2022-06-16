# Create publish pack
dotnet publish --configuration Release --use-current-runtime --self-contained

# Copy res to publish folder
Copy-Item -r ./res ./bin/Release/net6.0/win-x64/publish/ -Force

# Copy libs to publish folder
Copy-Item -r ./libs/win/* ./bin/Release/net6.0/win-x64/publish/ -Force

# Create folder publish in root dir
New-Item -ItemType Directory -Force ./publish

$filename = $args[0]

$compress = @{
    Path             = "./bin/Release/net6.0/win-x64/publish/*"
    CompressionLevel = "Fastest"
    DestinationPath  = "./publish/$filename.zip"
    Force            = $true
}
Compress-Archive @compress

# Copy the zip to clipboard
Set-Clipboard -Path ./publish/$filename.zip