# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Conventional Commits](https://www.conventionalcommits.org/).

## [1.5.0] - 2025-04-06
### Added
- Authentication system using Entra ID (formerly Azure AD)
- User-specific data isolation for subscriptions and categories
- JWT-based communication between frontend and backend
- Login/logout functionality with user profile display
- Route protection with authentication guards
- User management service for handling user creation and retrieval
- Sharing option for subscriptions with contact information field

### Changed
- Updated data models to include user ownership
- Modified API endpoints to filter data by the current user
- Enhanced frontend components to support authenticated requests
- Improved security with proper token handling and validation
- Renamed SubscriptionDTO to SubscriptionViewModel for better naming convention
- Refactored API service integration across multiple components

### Fixed
- Resolved 400 error when creating new categories
- Fixed subscription creation error by adding proper DTO handling
- Enhanced user management integration with frontend components
- Updated frontend build process to correctly use API_URL in config.js
- Simplified frontend deployment process using Azure Static Web Apps action

### CI/CD
- Streamlined tag handling in deployment workflows
- Updated frontend deployment to use Static Web Apps CLI
- Improved token masking for secure deployments

### Database
- Added User entity with Entra ID integration
- Created foreign key relationships between users and their data
- Added database migration script for seamless updates
- Ensured DateOnly properties use proper 'date' data type in SQL Server

## [1.4.0] - 2025-04-05
### Added
- Dark mode support with automatic system preference detection
- Custom color picker UI for categories
- 15 predefined color suggestions for quick selection
- Dynamic color display in calendar view for better visual identification

### Changed
- Optimized Azure infrastructure deployment using modular Bicep templates
- Updated Static Web App to global deployment mode for improved access speed
- Refactored resource naming conventions for consistency
- Enhanced form validation messages for clearer user feedback
- Improved calendar legend display with color indicators

### Fixed
- Category synchronization between list and calendar views
- CI workflow trigger conditions to only execute on src directory changes
- Display issues with buttons and calendar components in dark mode

### CI/CD
- Added tag-based release deployment workflow for automated version releases
- Optimized build process with separate frontend and backend deployment steps
- Improved post-deployment verification mechanism

## [1.3.0] - 2025-03-25
### Added
- Custom color code support for categories with color picker UI
- 15 predefined color suggestions in category form
- Dynamic color display in calendar view
- Color code validation in category API endpoints

### Changed
- Updated category model to include colorCode field
- Enhanced calendar legend display with color indicators
- Improved form validation messages for color input

### Fixed
- Category synchronization between list and calendar views
