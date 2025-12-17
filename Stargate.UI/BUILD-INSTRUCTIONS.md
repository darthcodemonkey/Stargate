# Angular UI Build Instructions

## Issue: 404 When Launching Website

The Angular UI application needs to be built before the ASP.NET Core server can serve it. Currently, the build script is disabled to prevent build failures when npm is not available.

## Solution Options

### Option 1: Build Angular App Manually (Recommended for First Run)

1. Navigate to the Angular client folder:
   ```powershell
   cd Stargate.UI\stargate.ui.client
   ```

2. Install dependencies (if not already done):
   ```powershell
   npm install
   ```

3. Build the Angular application:
   ```powershell
   npm run build
   ```

4. This will create the `dist\stargate.ui.client\browser\` folder with the built files.

5. Now run the Aspire AppHost - the UI should load correctly.

### Option 2: Enable Auto-Build (Requires npm in PATH)

If npm is installed and available in your system PATH:

1. Edit `Stargate.UI\stargate.ui.client\Stargate.UI.Client.esproj`
2. Change `<ShouldRunBuildScript>false</ShouldRunBuildScript>` to `<ShouldRunBuildScript>true</ShouldRunBuildScript>`
3. The Angular app will build automatically when you build the server project.

### Option 3: Development Mode with Angular Dev Server

For development, you can run the Angular dev server separately:

1. In one terminal, start the Angular dev server:
   ```powershell
   cd Stargate.UI\stargate.ui.client
   npm start
   ```

2. The Angular app will run on `https://localhost:54421` (configured in angular.json)

3. Update the proxy configuration if needed to point to the API

## Verification

After building, verify the build output exists:
```powershell
Test-Path "Stargate.UI\stargate.ui.client\dist\stargate.ui.client\browser\index.html"
```

This should return `True` if the build was successful.

## Notes

- The build output folder is: `Stargate.UI\stargate.ui.client\dist\stargate.ui.client\browser\`
- ASP.NET Core automatically includes static web assets from referenced .esproj projects
- The server is configured to serve files from this location and fall back to index.html for Angular routing

