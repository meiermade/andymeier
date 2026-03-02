# Andy Meier

[![Deploy](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml/badge.svg)](https://github.com/meiermade/andymeier/actions/workflows/deploy.yml)

Andy Meier's personal website and portfolio showcasing projects, professional services, and technical articles with a focus on functional programming and modern software architecture.

## About

This is a modern web application built using functional programming principles. The site serves as both a portfolio and a platform for sharing knowledge about software development, with particular emphasis on functional programming approaches and modern architectural patterns.

## Tech Stack

**Frontend:**
- F# with Giraffe web framework
- Datastar for reactive web components
- Tailwind CSS for styling
- FSharp.ViewEngine for server-side HTML generation

**Backend & Data:**
- SQLite database
- Notion API integration
- Markdig for markdown processing

**Infrastructure:**
- Docker containerization
- Kubernetes orchestration
- AWS ECR for container registry
- Cloudflare for CDN and DNS
- GitHub Actions for CI/CD

## Project Structure

```
andymeier/
├── app/                     # F# Web Application
│   ├── src/
│   │   ├── App/            # Main web application
│   │   ├── Domain/         # Domain logic & data access
│   │   ├── Build/          # FAKE build scripts
│   │   └── Tests/          # Expecto tests
│   └── fake.sh             # Build script runner
├── pulumi/                 # Infrastructure as Code
└── .github/workflows/      # GitHub Actions CI/CD
```

## Development

Requires .NET 8.0 SDK. To start development:

```bash
cd app
dotnet tool restore
dotnet paket restore
./fake.sh Watch
```

## License

This project is available under the [MIT License](LICENSE).