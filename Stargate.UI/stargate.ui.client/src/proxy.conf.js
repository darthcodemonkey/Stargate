const { env } = require('process');

// Get the API service URL from Aspire environment or fallback to default
const apiTarget = env["services__stargate-api__https__0"] 
  ?? env["services__stargate-api__http__0"]
  ?? 'https://localhost:5001';

const PROXY_CONFIG = [
  {
    context: [
      "/api"
    ],
    target: apiTarget,
    secure: false,
    changeOrigin: true,
    logLevel: "debug"
  }
]

module.exports = PROXY_CONFIG;
