#!/bin/sh

if ! command -v flatpak-builder &> /dev/null
then
    echo "flatpak-builder could not be found"
    exit
fi

if ! command -v dotnet &> /dev/null
then
    echo "dotnet could not be found"
    exit
fi

# Create a new build directory
rm -r flatpak_build/ # Remove the old build directory
mkdir -p flatpak_build/

# Build the app
dotnet publish -c Release -o flatpak_build -r linux-x64 -p:PublishSingleFile=true

cp -r data/* flatpak_build/ # Copy the data directory to the build directory

# Build the flatpak
cd flatpak_build/
flatpak-builder --repo=repo --force-clean build-dir org.CounterStats.yml --install --user

# Export flatpak to file
flatpak build-bundle repo CounterStats.flatpak org.counterstats.CounterStats --runtime-repo=https://flathub.org/repo/flathub.flatpakrepo

#finnish
echo
echo Your flatpak file is in flatpak_build/CounterStats.flatpak
echo
echo You can run CounterStats with: flatpak run org.counterstats.CounterStats