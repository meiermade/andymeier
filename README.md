# andrewmeier.dev

Personal website built with F#, Giraffe, Datastar, and Tailwind CSS.

## Structure

- `app/` - F# web application
  - `src/App/` - Main application (Giraffe + Datastar)
  - `src/App/articles/` - Articles (markdown + assets)
  - `src/Build/` - FAKE build script
  - `src/Tests/` - Expecto tests
- `pulumi/` - Infrastructure as code (AWS ECR, Cloudflare, Kubernetes)

## Articles

Articles are stored as subdirectories under `app/src/App/articles/`. Each article directory contains:
- `index.md` - Article content with YAML frontmatter
- Any image/asset files referenced in the markdown

### Directory naming

Directories use the format `yyyy-mm-dd-<permalink>`. The date prefix determines the article's created date.

### Frontmatter format

```yaml
---
title: My Article Title
summary: A brief description
thumbnail: thumb.jpg
cover: cover.jpg
tags: tag1, tag2
---
```

## Development

```bash
cd app
dotnet tool restore
dotnet paket restore
./fake.sh Watch
```

## Testing

```bash
cd app
dotnet run --project src/Tests/Tests.fsproj
```
