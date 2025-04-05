# subscription-tracker-client

## Project setup
```
npm install
```

### Compiles and hot-reloads for development
```
npm run serve
```

### Compiles and minifies for production
```
npm run build
```

### Lints and fixes files
```
npm run lint
```

### Environment Variables

The application uses environment variables for configuration. These can be set in the following ways:

1. **Local Development**: Create a `.env` file in the root of the project with the following variables:
   ```
   VUE_APP_API_BASE_URL=https://localhost:7052
   ```

2. **Production Build**: The application will use the value of `API_URL` environment variable during the build process.

3. **Azure Static Web App**: Environment variables can be configured in the Azure Portal:
   - Go to your Static Web App resource
   - Navigate to "Configuration" > "Application settings"
   - Add the environment variable `API_URL` with the URL of your API

### Customize configuration
See [Configuration Reference](https://cli.vuejs.org/config/).
