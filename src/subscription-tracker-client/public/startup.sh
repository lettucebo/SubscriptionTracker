#!/bin/bash

# This script is used to start the application with environment variables
# It can be used in Azure Web App on Linux

# Log startup information
echo "Starting Subscription Tracker Frontend..."
echo "Node version: $(node -v)"
echo "NPM version: $(npm -v)"

# Check if API_URL is set
if [ -z "$API_URL" ]; then
  echo "WARNING: API_URL environment variable is not set. Using default value from config.js."
else
  echo "API_URL is set to: $API_URL"
fi

# Start the server
node server.js
