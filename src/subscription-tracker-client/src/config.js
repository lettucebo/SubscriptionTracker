export const config = {
  baseUrl: 'https://localhost:7052',
  auth: {
    clientId: 'bff69ff1-dbac-43ef-88a1-f2a0c9aba3dc', // Replace with your Entra ID app registration client ID
    authority: 'https://login.microsoftonline.com/87befcf7-8c47-4964-9582-2942bfc01159',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  }
}
