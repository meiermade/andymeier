FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-amd64 AS builder

WORKDIR /app

# install tailwindcss v4 CLI
RUN curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/download/v4.1.18/tailwindcss-linux-x64 \
    && mv tailwindcss-linux-x64 /usr/local/bin/tailwindcss \
    && chmod +x /usr/local/bin/tailwindcss

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install && dotnet paket restore

# copy projects and build script
COPY src src
COPY fake.sh .

# publish the server 
RUN ./fake.sh Publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runner

WORKDIR /app

# copy compiled code from build image
COPY --from=builder /app/src/App/out .

ENTRYPOINT ["dotnet", "App.dll"]
