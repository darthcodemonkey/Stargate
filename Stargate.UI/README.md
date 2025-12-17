# Stargate UI Project

## Automatic Build Configuration

The Angular UI application is configured to build automatically when you build or run the project in Visual Studio.

### How It Works

1. **Automatic Build**: The `Stargate.UI.Client.esproj` has `<ShouldRunBuildScript>true</ShouldRunBuildScript>` enabled, which automatically runs `npm run build` when the project builds.

2. **Project Reference**: The `Stargate.UI.Server` project references the Angular client project, so building the server automatically triggers the Angular build.

3. **Node.js Requirement**: Visual Studio requires Node.js to be installed. The build process will use Node.js from:
   - System PATH
   - `C:\Program Files\nodejs\` (default installation location)
   - Visual Studio's bundled Node.js (if available)

### Prerequisites

- **Node.js**: Must be installed and available (typically at `C:\Program Files\nodejs\`)
- **npm**: Comes with Node.js installation
- **Angular CLI**: Installed via npm when you run `npm install` (first time only)

### First-Time Setup

On first build, npm will automatically install all dependencies from `package.json`. This may take a few minutes.

### Build Output

The Angular build output is located at:
```
Stargate.UI\stargate.ui.client\dist\stargate.ui.client\browser\
```

This folder contains the production-ready Angular application that is served by the ASP.NET Core server.

### Troubleshooting

If you get an error about npm not being found:

1. **Install Node.js**: Download and install from [nodejs.org](https://nodejs.org/)
2. **Verify Installation**: Open a new command prompt and run `node --version` and `npm --version`
3. **Restart Visual Studio**: After installing Node.js, restart Visual Studio to pick up the new PATH

If the build fails:

1. **Clean Solution**: Build → Clean Solution
2. **Delete node_modules**: Delete `Stargate.UI\stargate.ui.client\node_modules` folder
3. **Rebuild**: Build → Rebuild Solution

The build process will automatically run `npm install` if `node_modules` doesn't exist.

### Manual Build (If Needed)

If you need to manually build the Angular app:

```powershell
cd Stargate.UI\stargate.ui.client
npm install  # Only needed first time or after dependency changes
npm run build
```

### Development vs Production

- **Development**: When running from Visual Studio/Aspire, the built Angular files are served
- **Production**: The same built files are deployed and served statically

The build output is optimized for production with:
- Minification
- Tree-shaking
- Bundle optimization
- Source maps (in development builds)

## API Integration

The Angular UI communicates with the Stargate API through a proxy middleware in the UI.Server.

### Proxy Middleware

The UI.Server includes a custom proxy middleware that:
- Intercepts all `/api/*` requests from the Angular app
- Forwards them to the Stargate.API service using Aspire service discovery
- Handles POST/PUT/PATCH requests with proper body content
- Maintains request headers and forwards responses

**Location**: `Stargate.UI.Server/Program.cs`

### API Endpoints

The Angular services call these endpoints:
- `GET /api/person` - List all people
- `GET /api/person/{name}` - Get person by name
- `POST /api/person` - Create person
- `PUT /api/person/{name}` - Update person
- `GET /api/AstronautDuty/{name}` - Get duties by person name
- `POST /api/AstronautDuty` - Create duty

**Note**: Routes use PascalCase (e.g., `/api/AstronautDuty`) to match ASP.NET Core controller route conventions (`api/[controller]`).

### CORS Configuration

The API has CORS enabled to allow cross-origin requests from the UI. This is configured in `Stargate.Api/Program.cs`.

