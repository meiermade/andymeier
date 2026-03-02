# Andy Meier

[![Deploy](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml/badge.svg)](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml)

> Personal website and portfolio for Andy Meier - showcasing projects, services, and technical articles built with functional programming principles.

## 🌐 About

This is Andy Meier's personal website built as a modern, performant web application using F# and functional programming principles. The site serves as both a portfolio showcase and a technical demonstration of modern web development practices, featuring clean architecture, responsive design, and interactive elements powered by Datastar.

**Visit:** [andymeier.dev](https://andymeier.dev) | **Previous:** andrewmeier.dev (redirects automatically)

## ✨ Features

- **📂 Portfolio Showcase** - Featured projects with detailed descriptions and technical insights  
- **🛠 Service Offerings** - Professional services and technical expertise areas
- **📝 Technical Blog** - In-depth articles covering software development, functional programming, and architecture
- **📱 Responsive Design** - Mobile-first approach with adaptive layouts for all devices
- **⚡ Fast Performance** - Server-side rendered with minimal JavaScript and optimized loading
- **🎨 Modern UI/UX** - Clean design with dark/light theme toggle and smooth interactions
- **🔄 Interactive Elements** - Enhanced user experience with Datastar reactive framework
- **📊 Observability** - Comprehensive telemetry and monitoring with OpenTelemetry
- **🔍 SEO Optimized** - Semantic HTML and meta tags for search engine visibility

### Recent Feature Updates
- **🍔 Mobile Navigation** - Responsive hamburger menu with smooth overlay transitions  
- **🌙 Theme Toggle** - Dark/light mode switching with user preference persistence
- **📐 Enhanced Layout** - Updated navigation dropdowns and improved spacing
- **💨 Performance** - Lazy loading for syntax highlighting and optimized Tailwind usage

## 🛠 Tech Stack

### Frontend
- **F#** - Primary language leveraging functional programming paradigms
- **Giraffe** - F# web framework built on ASP.NET Core for fast, composable web apps  
- **Datastar** - Modern reactive web framework for enhanced UX without heavy JavaScript
- **Tailwind CSS v4** - Utility-first CSS framework with custom design system
- **FSharp.ViewEngine** - Type-safe server-side HTML generation
- **Markdig** - Powerful Markdown processing with syntax highlighting

### Backend & Data
- **SQLite** - Embedded database for fast local data storage
- **Notion API** - Integrated content management for dynamic content updates
- **ASP.NET Core** - High-performance web server and API framework

### Infrastructure & Deployment  
- **Docker** - Multi-stage containerization with optimized images
- **Kubernetes** - Container orchestration for scalable deployments
- **AWS ECR** - Private container registry for secure image storage
- **Cloudflare** - Global CDN, DNS management, and DDoS protection
- **Pulumi** - Infrastructure as Code with TypeScript for reproducible deployments
- **GitHub Actions** - Automated CI/CD pipeline with preview and production workflows

### Development & Testing
- **FAKE** - F# build automation and task runner
- **Expecto** - F# testing framework for unit and integration tests  
- **Paket** - Dependency management with precise version control
- **OpenTelemetry** - Distributed tracing and application monitoring

## 🚀 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Node.js 24.x LTS](https://nodejs.org/) (required for Pulumi infrastructure)
- [Docker Desktop](https://docker.com/) (for containerization and local testing)
- [Git](https://git-scm.com/) (for version control)

**Optional but recommended:**
- [Visual Studio Code](https://code.visualstudio.com/) with [Ionide](https://ionide.io/) extension for F# development
- [Pulumi CLI](https://www.pulumi.com/docs/install/) (for infrastructure management)

## 💻 Development Setup

### Quick Start

1. **Clone the Repository**
   ```bash
   git clone https://github.com/meiermade/andymeier.git
   cd andymeier/app
   ```

2. **Restore Tools and Dependencies**
   ```bash
   dotnet tool restore          # Install FAKE and other tools
   dotnet paket install         # Install F# packages
   dotnet paket restore         # Restore dependencies
   ```

3. **Start Development Server**
   ```bash
   ./fake.sh Watch              # Starts with hot reload
   ```
   
   The application will be available at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP) with automatic browser opening and hot reload enabled.

### Development Workflow

The `fake.sh` script provides several development commands:

```bash
./fake.sh Watch          # Start with hot reload (recommended)
./fake.sh Build          # Build the application  
./fake.sh Test           # Run all tests
./fake.sh Publish        # Create production build
./fake.sh Clean          # Clean build artifacts
```

### Environment Configuration

The application supports multiple environments through configuration:

- **Development** - Local development with hot reload
- **Production** - Optimized build for deployment
- **Testing** - Isolated environment for running tests

Configuration is handled through:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Environment variables for sensitive data

### Docker Development

For a containerized development experience:

```bash
cd app
docker-compose up --build    # Build and run with docker-compose
```

## 🧪 Testing

### Running Tests

```bash
cd app
./fake.sh Test               # Run all tests with coverage
```

### Test Structure

The test suite includes:
- **Unit Tests** - Domain logic and pure functions
- **Integration Tests** - Database operations and external APIs  
- **Web Tests** - HTTP endpoints and routing
- **Property Tests** - Fuzz testing with Expecto's property-based testing

Tests are located in `src/Tests/` and use the Expecto framework for a functional testing approach.

## 📁 Project Structure

```
andymeier/
├── 📁 app/                          # F# Web Application Root
│   ├── 📁 src/
│   │   ├── 📁 App/                  # Main Web Application
│   │   │   ├── 📁 src/
│   │   │   │   ├── 📁 Index/        # Homepage handlers & views
│   │   │   │   ├── 📁 Projects/     # Project portfolio showcase
│   │   │   │   ├── 📁 Services/     # Professional services pages
│   │   │   │   ├── 📁 Articles/     # Technical blog & content
│   │   │   │   └── 📁 Common/       # Shared components & layouts
│   │   │   ├── 📄 Config.fs         # Application configuration
│   │   │   ├── 📄 Program.fs        # Application entry point & setup
│   │   │   ├── 📄 Infrastructure.fs # Dependency injection & services
│   │   │   └── 📄 Services.fs       # Business logic services
│   │   ├── 📁 Domain/               # Core Domain Logic
│   │   │   ├── 📄 Article.fs        # Article domain models
│   │   │   ├── 📄 Notion.fs         # Notion API integration
│   │   │   ├── 📄 Sqlite.fs         # Database operations
│   │   │   ├── 📄 Telemetry.fs      # Observability & monitoring
│   │   │   └── 📄 Infrastructure.fs # Domain infrastructure
│   │   ├── 📁 Build/                # FAKE Build Scripts
│   │   │   └── 📄 Program.fs        # Build automation logic
│   │   └── 📁 Tests/                # Test Suite
│   │       └── 📄 *.fs              # Expecto test modules
│   ├── 📄 paket.dependencies        # Package dependencies
│   ├── 📄 paket.lock                # Locked dependency versions
│   ├── 📄 fake.sh                   # Build script runner (Unix)
│   ├── 📄 Dockerfile                # Multi-stage container build
│   └── 📄 docker-compose.yml        # Local containerized development
├── 📁 pulumi/                       # Infrastructure as Code
│   ├── 📁 src/                      # Pulumi TypeScript modules  
│   ├── 📄 index.ts                  # Main infrastructure definition
│   ├── 📄 package.json              # Node.js dependencies
│   ├── 📄 Pulumi.yaml               # Pulumi project configuration
│   └── 📄 Pulumi.prod.yaml          # Production stack configuration
├── 📁 .github/
│   └── 📁 workflows/                # CI/CD Automation
│       ├── 📄 deploy.yml            # Production deployment pipeline
│       └── 📄 preview.yml           # Preview/staging deployment
└── 📄 README.md                     # This documentation
```

### Key Architecture Patterns

- **Functional-First Design** - Immutable data structures and pure functions where possible
- **Handler-View Pattern** - Clean separation between request handling and view rendering  
- **Domain-Driven Design** - Clear domain boundaries with the `Domain` project
- **Dependency Injection** - Services registered and injected through ASP.NET Core DI
- **Configuration-Based** - Environment-specific settings through configuration files

## 🚢 Deployment

### Automated Deployment

The application uses a sophisticated CI/CD pipeline with GitHub Actions:

1. **🔍 Pull Request Workflow** (`preview.yml`)
   - Runs on every PR to validate changes
   - Executes full test suite
   - Validates infrastructure changes with `pulumi preview`
   - Provides deployment preview without actual deployment

2. **🚀 Production Deployment** (`deploy.yml`)
   - Triggers on pushes to `main` branch
   - Builds optimized Docker image
   - Pushes to AWS ECR private registry
   - Updates Kubernetes deployment via Pulumi
   - Manages Cloudflare DNS and CDN settings

### Infrastructure Overview

The production infrastructure includes:
- **Kubernetes Cluster** - Scalable container orchestration
- **Load Balancer** - Traffic distribution and SSL termination  
- **CDN Integration** - Global content delivery via Cloudflare
- **Container Registry** - Secure private registry on AWS ECR
- **DNS Management** - Automated DNS updates through Pulumi

### Manual Infrastructure Management

For infrastructure inspection and debugging:

```bash
cd pulumi
npm install                    # Install Pulumi dependencies
pulumi preview                 # Show planned infrastructure changes
pulumi stack output           # View current stack outputs
```

> **⚠️ Important**: Never run `pulumi up` manually in production. The CI/CD pipeline handles all deployments automatically upon merge to `main`.

### Environment Variables

The application requires the following environment variables in production:

- `ASPNETCORE_ENVIRONMENT` - Set to "Production"
- `ConnectionStrings__DefaultConnection` - Database connection string
- `NOTION_API_KEY` - Notion integration API key (if using Notion CMS)
- `OTEL_EXPORTER_OTLP_ENDPOINT` - OpenTelemetry collector endpoint

## 🤝 Contributing

We welcome contributions! Here's how to get started:

### Pre-Pull Request Checklist

Before creating a pull request, ensure all checks pass:

1. **✅ Code Quality**
   ```bash
   cd app
   ./fake.sh Build              # Verify clean build
   ```

2. **✅ Tests Pass**
   ```bash
   ./fake.sh Test               # Run full test suite
   ```

3. **✅ Infrastructure Valid**
   ```bash
   cd ../pulumi
   pulumi preview               # Validate infrastructure changes
   ```

### Development Workflow

1. **🔀 Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **💻 Make Changes**
   - Follow F# coding conventions
   - Write tests for new functionality
   - Update documentation if needed

3. **🧪 Test Locally**
   ```bash
   ./fake.sh Watch              # Test in development mode
   ./fake.sh Test               # Run test suite
   ```

4. **📝 Create Pull Request**
   - Provide clear description of changes
   - Reference any related issues
   - Ensure CI checks pass

### Code Style Guidelines

- **F# Conventions** - Follow standard F# formatting and naming conventions
- **Functional Principles** - Prefer immutability and pure functions
- **Type Safety** - Leverage F#'s type system for compile-time safety
- **Documentation** - Use XML documentation comments for public APIs
- **Testing** - Maintain high test coverage, especially for domain logic

## 🎯 Future Enhancements

### Planned Features
- **📑 Enhanced Navigation** - Table of contents sidebar for articles with jump-to-section functionality
- **📊 Reading Progress** - Progress indicators and back/forward navigation for long-form content  
- **🎨 Interactive Elements** - Additional Datastar-powered interactions and animations
- **⚡ Performance Optimization** - Further improvements to Core Web Vitals and loading times
- **📱 PWA Support** - Progressive Web App capabilities for offline reading
- **🔍 Search Functionality** - Full-text search across articles and projects
- **💬 Comments System** - Integration with external comment providers
- **📈 Analytics** - Privacy-focused analytics and visitor insights

### Technical Improvements
- **🏗 Build Optimization** - Incremental builds and caching improvements
- **📦 Bundle Optimization** - Code splitting and lazy loading for JavaScript
- **🔐 Security Enhancements** - Content Security Policy and additional security headers
- **♿ Accessibility** - Enhanced keyboard navigation and screen reader support
- **🌐 Internationalization** - Multi-language support infrastructure

## 🔧 Troubleshooting

### Common Issues

**Build Failures**
```bash
# Clean and restore if build fails
./fake.sh Clean
dotnet paket restore
./fake.sh Build
```

**Port Conflicts**
```bash
# Check for processes using ports 5000/5001
lsof -i :5000
lsof -i :5001
```

**Docker Issues**
```bash
# Reset Docker state
docker system prune -f
docker-compose down -v
docker-compose up --build
```

**Tool Restoration Issues**
```bash
# Reset .NET tools
rm -rf .config
dotnet tool restore
```

### Getting Help

- **Issues** - Report bugs or request features via [GitHub Issues](https://github.com/meiermade/andymeier/issues)
- **Discussions** - Ask questions in [GitHub Discussions](https://github.com/meiermade/andymeier/discussions)  
- **Contact** - Reach out through the website's contact section

## 📧 Contact

For questions, suggestions, collaboration opportunities, or technical discussions:

- **Website** - [andymeier.dev](https://andymeier.dev)
- **GitHub** - [@meiermade](https://github.com/meiermade)
- **Email** - Contact form available on the website

## 🙏 Acknowledgments

Built with these fantastic open-source technologies:
- **F# Community** - For the amazing functional programming ecosystem
- **Giraffe** - Elegant F# web framework
- **Datastar** - Modern reactive web framework  
- **Tailwind CSS** - Utility-first CSS framework
- **Pulumi** - Infrastructure as Code platform

## 📜 License

This project is open source and available under the [MIT License](LICENSE).

---

Built with ❤️ using F# and functional programming principles by [Andy Meier](https://andymeier.dev)