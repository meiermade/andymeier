# Andy Meier

[![Deploy](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml/badge.svg)](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml)

> Personal website and portfolio for Andy Meier - showcasing projects, services, and technical articles with a focus on functional programming and modern software architecture.

## 🎯 What Conversation Are We Having?

This website is designed to start meaningful conversations about:

- **Building Better Software** - Through functional programming principles and modern architectural patterns
- **Solving Real Problems** - With practical projects and professional services that deliver value
- **Learning Together** - Sharing insights, lessons learned, and technical knowledge with the community
- **Collaboration Opportunities** - Connecting with others who are passionate about quality software development

Whether you're a fellow developer exploring functional programming, a business looking for technical expertise, or someone curious about modern software development practices, this site aims to provide value through clear explanations, practical examples, and genuine insights.

## ✨ Visitor Experience

### What You'll Find Here

- **Portfolio Showcase** - Featured projects with detailed technical discussions and lessons learned
- **Professional Services** - Clear information about expertise and how we can work together
- **Technical Articles** - Deep dives into functional programming, architecture patterns, and development practices
- **Interactive Reading** - Enhanced navigation and reading experience designed for technical content

### Enhanced Reading Experience

The site is designed with modern content consumption in mind:

- **📑 Smart Navigation** - Table of contents sidebar for easy article navigation (inspired by [a16z's technical articles](https://a16z.com/emerging-architectures-for-modern-data-infrastructure/))
- **📊 Reading Progress** - Visual progress indicators and intuitive back/forward navigation (similar to [Avanscoperta's blog experience](https://blog.avanscoperta.it/2020/08/04/domain-driven-design-in-2020/))
- **🚀 Fast & Responsive** - Server-side rendered for instant loading across all devices
- **🎨 Clean Design** - Focused on content with minimal distractions

## 🌐 About

This is Andy Meier's personal website built as a modern, performant web application using functional programming principles. The site serves as both a portfolio and a platform for sharing knowledge about software development, with particular emphasis on functional programming approaches and modern architectural patterns.

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

## 🎯 Planned User Experience Enhancements

### Reading & Navigation Features
- **📑 Sidebar Table of Contents** - Auto-generated TOC for articles with smooth scrolling navigation
  - Inspired by [a16z's technical article layout](https://a16z.com/emerging-architectures-for-modern-data-infrastructure/)
  - Sticky positioning for long-form content
  - Progress indicators showing reading completion

- **📊 Reading Progress & Controls** - Enhanced article reading experience
  - Reading progress bar (similar to [Avanscoperta's blog](https://blog.avanscoperta.it/2020/08/04/domain-driven-design-in-2020/))
  - "Back to top" floating button for easy navigation
  - Previous/next article navigation
  - Estimated reading time display

### Interactive Elements
- **🔍 Enhanced Search** - Full-text search across projects and articles
- **🏷️ Smart Tagging** - Category-based filtering and related content suggestions
- **💬 Engagement Features** - Comment integration for technical discussions
- **📱 Mobile-First** - Optimized touch interactions and responsive design

### Performance & Accessibility
- **⚡ Progressive Enhancement** - Core functionality works without JavaScript
- **♿ Accessibility First** - WCAG 2.1 AA compliance
- **🌙 Theme Support** - Dark/light mode preferences
- **📊 Analytics & Insights** - Understanding visitor engagement patterns

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

## 📧 Contact

For questions, suggestions, or collaboration opportunities, feel free to reach out through the website's contact section.

## 📜 License

This project is open source and available under the [MIT License](LICENSE).

---

Built with ❤️ using F# and functional programming principles - because elegant code leads to elegant solutions.