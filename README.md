# andrewmeier.dev

Personal website built with F#, Giraffe, Datastar, and Tailwind CSS.

## Structure

- `app/` - F# web application
  - `src/App/` - Main application (Giraffe + Datastar)
  - `src/Build/` - FAKE build script
  - `src/Tests/` - Expecto tests
- `pulumi/` - Infrastructure as code (AWS ECR, Cloudflare, Kubernetes)

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
./fake.sh Test
```
