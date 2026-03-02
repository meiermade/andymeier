# Andy Meier

[![Deploy](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml/badge.svg)](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml)

> Personal website and portfolio for Andy Meier - showcasing projects, services, and technical articles.

## 🌐 About

This is Andy Meier's personal website built as a modern, performant web application using functional programming principles. The site features a clean, responsive design with sections for projects, services, and technical writing.

## ✨ Features

- **Portfolio Showcase** - Featured projects with detailed descriptions
- **Service Offerings** - Professional services and expertise
- **Technical Blog** - Articles and insights
- **Responsive Design** - Optimized for all devices
- **Fast Performance** - Server-side rendered with minimal JavaScript
- **Modern Architecture** - Built with functional programming principles

## 🛠 Tech Stack

### Frontend
- **F#** - Primary language using functional programming
- **Giraffe** - F# web framework built on ASP.NET Core
- **Datastar** - Reactive web framework for enhanced UX
- **Tailwind CSS** - Utility-first CSS framework
- **FSharp.ViewEngine** - Server-side HTML generation

### Backend & Data
- **SQLite** - Local database
- **Notion API** - Content management integration
- **Markdig** - Markdown processing

### Infrastructure & Deployment
- **Docker** - Containerization
- **Kubernetes** - Container orchestration
- **AWS ECR** - Container registry
- **Cloudflare** - CDN and DNS
- **Pulumi** - Infrastructure as Code
- **GitHub Actions** - CI/CD pipeline

### Development & Testing
- **FAKE** - F# build automation
- **Expecto** - F# testing framework
- **Paket** - F# dependency management

## 🚀 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 24.x](https://nodejs.org/) (for Pulumi infrastructure)
- [Docker](https://docker.com/) (for containerization)

## 💻 Development Setup

### 1. Clone the Repository
```bash
git clone https://github.com/meiermade/andymeier.git
cd andymeier
```

### 2. Install .NET Tools and Dependencies
```bash
cd app
dotnet tool restore
dotnet paket restore
```

### 3. Start Development Server
```bash
./fake.sh Watch
```

The application will start with hot reloading enabled. Open your browser and navigate to the displayed local URL.

### 4. Install Infrastructure Dependencies (Optional)
```bash
cd ../pulumi
npm install
```

## 🧪 Testing

Run the complete test suite:

```bash
cd app
./fake.sh Test
```

## 📁 Project Structure

```
andymeier/
├── app/                          # F# Web Application
│   ├── src/
│   │   ├── App/                  # Main web application
│   │   │   ├── src/
│   │   │   │   ├── Index/        # Homepage handlers & views
│   │   │   │   ├── Projects/     # Project showcase
│   │   │   │   ├── Services/     # Service offerings
│   │   │   │   ├── Articles/     # Blog/articles
│   │   │   │   └── Common/       # Shared components
│   │   │   ├── Config.fs         # Application configuration
│   │   │   ├── Program.fs        # Application entry point
│   │   │   └── ...
│   │   ├── Domain/               # Domain logic & data access
│   │   │   ├── Notion.fs         # Notion API integration
│   │   │   ├── Sqlite.fs         # Database operations
│   │   │   └── ...
│   │   ├── Build/                # FAKE build scripts
│   │   └── Tests/                # Expecto tests
│   ├── paket.dependencies        # Package dependencies
│   └── fake.sh                   # Build script runner
├── pulumi/                       # Infrastructure as Code
│   ├── src/                      # Pulumi TypeScript modules  
│   ├── index.ts                  # Main infrastructure definition
│   └── package.json              # Node.js dependencies
├── .github/workflows/            # GitHub Actions CI/CD
└── README.md                     # This file
```

## 🚢 Deployment

The application uses automated deployment via GitHub Actions:

1. **Automatic Deployment**: Every push to the `main` branch triggers a deployment
2. **Infrastructure Management**: Pulumi handles all infrastructure provisioning and updates
3. **Container Registry**: Docker images are stored in AWS ECR
4. **Kubernetes Deployment**: Application runs on Kubernetes cluster
5. **CDN**: Cloudflare provides global content delivery

### Manual Infrastructure Preview
```bash
cd pulumi
pulumi preview  # Shows planned infrastructure changes
```

> **⚠️ Important**: Never run `pulumi up` manually - the CI/CD pipeline handles this automatically on merge to main.

## 🤝 Contributing

### Pre-Pull Request Checklist

Before creating a pull request, ensure:

1. **Tests Pass**: `cd app && ./fake.sh Test`
2. **Infrastructure Valid**: `cd pulumi && pulumi preview` (no errors)
3. **Code Quality**: Follow F# formatting conventions

### Development Workflow

1. Create a feature branch from `main`
2. Make your changes
3. Run tests and infrastructure preview
4. Create a pull request with a clear description
5. Wait for automated checks to pass

## 🎯 Future Enhancements

- **Enhanced Navigation**: Table of contents sidebar for better content discovery
- **Reading Progress**: Progress indicators and back/forward buttons for articles
- **Interactive Elements**: Enhanced user interactions with Datastar
- **Performance Optimization**: Further improvements to loading times
- **Content Expansion**: Additional project showcases and technical articles

## 📧 Contact

For questions, suggestions, or collaboration opportunities, feel free to reach out through the website's contact section.

## 📜 License

This project is open source and available under the [MIT License](LICENSE).

---

Built with ❤️ using F# and functional programming principles.