const express = require('express');
const path = require('path');
const history = require('connect-history-api-fallback');
const fs = require('fs');

const app = express();
const port = process.env.PORT || 8080;

// Environment variables to expose to the frontend
const envVars = {
  API_URL: process.env.API_URL,
  AUTH_CLIENT_ID: process.env.AUTH_CLIENT_ID,
  AUTH_AUTHORITY: process.env.AUTH_AUTHORITY,
  APPLICATIONINSIGHTS_CONNECTION_STRING: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
};

// Filter out undefined environment variables
const filteredEnvVars = Object.fromEntries(
  Object.entries(envVars).filter(([_, value]) => value !== undefined)
);

// Create a middleware to inject environment variables
app.use((req, res, next) => {
  if (req.path === '/env.js') {
    res.setHeader('Content-Type', 'application/javascript');
    res.send(`window.__env = ${JSON.stringify(filteredEnvVars, null, 2)};`);
  } else {
    next();
  }
});

// Use history API fallback middleware
app.use(history());

// Serve static files
app.use(express.static(path.join(__dirname)));

// Inject the env.js script into index.html if it exists
app.get('/', (req, res) => {
  const indexPath = path.join(__dirname, 'index.html');
  fs.readFile(indexPath, 'utf8', (err, data) => {
    if (err) {
      console.error('Error reading index.html:', err);
      return res.status(500).send('Error loading application');
    }

    // Inject the env.js script before the closing </head> tag
    const modifiedData = data.replace(
      '</head>',
      `  <script src="/env.js"></script>\n</head>`
    );

    res.send(modifiedData);
  });
});

// All other routes should serve the index.html file
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'index.html'));
});

// Start the server
app.listen(port, () => {
  console.log(`Server running on port ${port}`);
  console.log('Environment variables loaded:', Object.keys(filteredEnvVars));
});
