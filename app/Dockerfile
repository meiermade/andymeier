FROM --platform=$BUILDPLATFORM node:24.11.1-alpine AS browser-builder

WORKDIR /app/src/App
COPY src/App/package.json src/App/package-lock.json ./
RUN npm ci --ignore-scripts
COPY src/App/Client Client
COPY src/App/Components Components
COPY src/App/src src
COPY src/App/input.css src/App/tsconfig.json ./
RUN npm run check \
    && npm test \
    && npm run build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS builder

WORKDIR /app

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install && dotnet paket restore

# copy projects and the single npm-built asset set
COPY src src
COPY --from=browser-builder /app/src/App/wwwroot/ src/App/wwwroot/
COPY fake.sh .
RUN chmod +x ./fake.sh

# publish the server without rebuilding assets
RUN SKIP_BROWSER_BUILD=true ./fake.sh Publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runner

WORKDIR /app

COPY --from=builder /app/src/App/out .

ENTRYPOINT ["dotnet", "App.dll"]
