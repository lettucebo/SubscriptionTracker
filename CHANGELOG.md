# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Conventional Commits](https://www.conventionalcommits.org/).

## [1.5.8] - 2025-05-25
### Security
- Upgraded Entity Framework Core related packages from 8.0.5 to 8.0.6 to fix security vulnerabilities
- Further improved security compliance of package dependencies

## [1.5.7] - 2025-05-25
### Security
- Upgraded Entity Framework Core related packages from 8.0.3 to 8.0.5 to fix security vulnerabilities
- Upgraded axios package from 1.6.7 to 1.6.8 to fix security vulnerabilities
- Improved security compliance of package dependencies

## [1.5.6] - 2025-04-09
### Added
- Azure Application Insights integration for comprehensive monitoring
- OpenTelemetry SDK implementation in backend API
- Application Insights JavaScript SDK integration in frontend
- Automatic tracking of page views, HTTP requests, and exceptions
- Custom telemetry service for tracking application-specific events
- Detailed documentation for Application Insights implementation

### Security
- Moved Application Insights connection string to user secrets
- Enhanced .gitignore rules to prevent committing sensitive configuration
- Implemented secure connection string handling in frontend

### Changed
- Simplified backend telemetry configuration for better compatibility
- Optimized frontend telemetry implementation for browser environments
- Enhanced error tracking with detailed context information

## [1.5.5] - 2025-04-08
### Changed
- Improved radar chart visibility in dark mode with enhanced contrast
- Added theme detection to CategoryComparison component for dynamic styling

### Fixed
- Enhanced radar chart grid lines with higher contrast and thicker lines in dark mode
- Improved data point visibility with larger points and white fill for better contrast
- Added subtle shadow effects for better visual hierarchy in dark mode
- Ensured proper label readability with bold text and backdrop colors

### UI/UX
- Optimized chart rendering for both dark and light modes
- Added smooth line transitions for better data visualization
- Implemented responsive point sizing based on current theme
- Enhanced hover effects with contrasting colors for better interaction

## [1.5.3] - 2025-04-08
### Added
- Subscription Lifecycle view for visualizing subscription timelines
- Timeline visualization with monthly scale for better temporal understanding
- Sorting options for subscriptions by start date, end date, name, category, or amount
- Category filtering in Subscription Lifecycle view
- Color-coded subscription bars matching their category colors
- Monthly price display on timeline bars for quick cost assessment
- Category legend with color indicators for easy reference

### Fixed
- Category filter error in Subscription Lifecycle view when changing selection
- Enhanced error handling in filtering functions to prevent crashes
- Improved type checking for subscription data to ensure consistent display

## [1.5.2] - 2025-04-08
### Added
- New Report page with subscription cost analytics and visualization
- Tabbed interface for Report page with "Overview" and "Category Breakdown" sections
- Pie chart visualization of costs by category using Chart.js
- Sorting options for categories by name or cost in Category Breakdown
- Colored headers in Category Breakdown matching each category's color
- Category colors displayed in Home page badges for better visual identification
- Separation line in categories for improved readability
- Home page link in navbar for easier navigation
- Comprehensive project guidelines with documentation requirements and naming conventions
- MCP configuration with GitHub, Google Calendar, Spotify, Firecrawl, Sentry, and Git server integrations

### Changed
- Renamed SubscriptionDTO to SubscriptionViewModel following project naming conventions
- Created new Models/ViewModels folder structure for better organization
- Enhanced authentication flow with improved error handling and redirection
- Updated frontend deployment settings to use Node.js 22
- Improved category badge styling with dynamic colors based on category

### Fixed
- Improved text visibility in all badges (Billing Cycle, Amount, Sharing, Status) for dark mode
- Enhanced text contrast with intelligent color calculation based on background brightness
- Fixed invisible Calendar View title and icon in light mode
- Maintained color of active navbar links in dark mode for better visibility
- Pre-selection of category when editing subscriptions
- Resolved 400 error when creating new categories by adding proper DTOs
- Fixed subscription creation error with proper model handling

### UI/UX
- Added text shadows for better contrast in dark mode
- Added borders for light-colored badges in dark mode
- Enhanced readability with white text on colored backgrounds
- Improved typography with better spacing and formatting
- Added new logo assets including SVG and PNG formats

## [1.5.1] - 2025-04-07
### Changed
- Migrated frontend from Azure Static Web App to Azure Web App for better integration
- Updated App Service Plan to use Linux instead of Windows for both frontend and backend
- Configured frontend to use Node.js 22 LTS runtime on Linux
- Configured backend to use .NET Core 8.0 on Linux
- Optimized infrastructure by sharing a single App Service Plan between frontend and backend

### Security
- Enabled VNet integration for both frontend and backend Web Apps
- Configured all outbound traffic to route through VNet for enhanced security
- Updated SQL Server connection string to use private endpoint DNS name
- Improved network isolation with proper subnet delegation for Web App integration

### Infrastructure
- Added dedicated integration subnet with proper delegations for Web App VNet integration
- Reorganized resource naming convention for better clarity (frontend/backend suffixes)
- Enhanced deployment dependencies to ensure proper resource creation order

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
