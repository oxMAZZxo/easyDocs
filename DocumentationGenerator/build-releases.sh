PROJECT_NAME="EasyDocs"
PUBLISH_DIR="publish"

set -e

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR/windows"
mkdir -p "$PUBLISH_DIR/linux"

# -------------------------------
# Windows Build
# -------------------------------
echo "Building Windows release..."
dotnet publish ./DocumentationGenerator.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "$PUBLISH_DIR/windows"

echo "Zipping Windows release..."
cd "$PUBLISH_DIR/windows"
zip -r "../${PROJECT_NAME}-windows.zip" *
cd ../../

# -------------------------------
# Linux Build
# -------------------------------
echo "Building Linux release..."
dotnet publish ./DocumentationGenerator.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "$PUBLISH_DIR/linux"

echo "Making Linux binary executable..."
chmod +x "$PUBLISH_DIR/linux/$PROJECT_NAME"

echo "Creating tar.gz for Linux..."
tar -czvf "$PUBLISH_DIR/$PROJECT_NAME-linux.tar.gz" -C "$PUBLISH_DIR/linux" "$PROJECT_NAME"

echo "Builds completed! Files in '$PUBLISH_DIR':"
ls "$PUBLISH_DIR"