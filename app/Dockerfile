FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS builder

WORKDIR /app

# install tailwindcss v4 CLI (match builder platform)
RUN ARCH=$(uname -m | sed 's/aarch64/arm64/' | sed 's/x86_64/x64/') \
    && curl -fsSLo /usr/local/bin/tailwindcss "https://github.com/tailwindlabs/tailwindcss/releases/download/v4.2.1/tailwindcss-linux-${ARCH}" \
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
RUN chmod +x ./fake.sh

# publish the server 
RUN ./fake.sh Publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runner

WORKDIR /app

# copy compiled code from build image
COPY --from=builder /app/src/App/out .

ENTRYPOINT ["dotnet", "App.dll"]
