# Security Vulnerability Remediation Plan - 2025/05/25

## Problem Description

Through Mend security scanning, the following security vulnerabilities were discovered in the SubscriptionTracker project:

1. Microsoft.EntityFrameworkCore related packages (version 8.0.3), including:
   - Microsoft.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.InMemory
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Design
   
2. axios package (version 1.6.7)

## Solution

Upgrade all vulnerable packages to the latest stable versions:

1. Upgrade Entity Framework Core related packages to 8.0.5
2. Upgrade axios to version 1.6.8

## Implementation Plan

### 1. Update .NET Packages

Update Entity Framework Core package versions in the following files:

- `SubscriptionTracker.Service.csproj`
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.InMemory
  - Microsoft.EntityFrameworkCore.SqlServer

- `SubscriptionTracker.Api.csproj`
  - Microsoft.EntityFrameworkCore.Design

### 2. Update Frontend Packages

Update the axios package version in the frontend `package.json` file.

### 3. Validation

- Ensure the application can build and run normally
- Verify that database migrations function correctly
- Confirm API call functionality is working properly

### 4. Documentation Updates

- Update CHANGELOG.md to document this security fix
- Track issue remediation progress on GitHub

## Related Issues

- GitHub Issue #14: microsoft.entityframeworkcore.8.0.3.nupkg security vulnerability
- GitHub Issue #15: microsoft.entityframeworkcore.inmemory.8.0.3.nupkg security vulnerability
- GitHub Issue #16: axios-1.6.7.tgz security vulnerability
- GitHub Issue #17: microsoft.entityframeworkcore.sqlserver.8.0.3.nupkg security vulnerability
