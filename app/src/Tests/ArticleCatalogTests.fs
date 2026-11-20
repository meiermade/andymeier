module ArticleCatalogTests

open App.Articles
open Expecto
open FSharp.ViewEngine

[<Tests>]
let articleCatalogTests =
    testList
        "Article catalog"
        [ test "contains the articles in reverse chronological order" {
              let permalinks = Catalog.all |> List.map _.permalink

              Expect.equal
                  permalinks
                  [ "generating-realistic-images-with-blender-and-openai"
                    "why-im-building-athleneum"
                    "bpc-157-elbow-research"
                    "personal-infrastructure"
                    "dev-env"
                    "fsharp-semantic-kernel" ]
                  "Expected the article catalog"
          }

          test "renders the Athleneum backstory with its public campus images and links" {
              let article = Catalog.tryFind "why-im-building-athleneum" |> Option.get
              let html = Render.toHtmlDocString article.page
              Expect.stringContains html "href=\"https://athleneum.com/\"" "Expected a link to the live site"
              Expect.stringContains html "href=\"https://athleneum.com/waitlist\"" "Expected the public waitlist link"
              Expect.stringContains html "https://www.rhodes.edu/campus-maps" "Expected the official campus map link"
              let campusAssets = "https://assets.meiermade.com/andymeier/articles/why-im-building-athleneum/"

              for file in
                  [ "about-rhodes-map-crop-6c5b62d9.webp"
                    "about-rhodes-refectory-07ba62c8.webp"
                    "about-rhodes-library-exterior-59df7ac5.webp"
                    "about-rhodes-athletics-cff96fcf.webp"
                    "about-rhodes-science-96c7d7b0.webp" ] do
                  Expect.stringContains html $"src=\"{campusAssets}{file}\"" $"Expected the public image {file}"

              Expect.isFalse (html.Contains "/images/drafts/") "The article must not use local draft URLs"
          }

          test "renders source-controlled article pages with durable assets" {
              let rendered =
                  Catalog.all
                  |> List.map (fun article -> article, Render.toHtmlDocString article.page)

              for article, html in rendered do
                  Expect.stringContains html article.title $"Expected {article.permalink} title"

                  let expectedAssetOrigin =
                      if article.permalink = "why-im-building-athleneum" then
                          "https://athleneum.com/images/"
                      else
                          "https://assets.meiermade.com/andymeier/articles/"

                  Expect.stringContains html expectedAssetOrigin $"Expected {article.permalink} public assets"

                  Expect.isFalse
                      (html.Contains "prod-files-secure.s3")
                      $"Expected {article.permalink} not to use signed Notion assets"

                  Expect.isFalse
                      (html.Contains "app.notion.com")
                      $"Expected {article.permalink} not to use Notion assets"

              let semanticKernel =
                  rendered
                  |> List.find (fun (article, _) -> article.permalink = "fsharp-semantic-kernel")
                  |> snd

              let infrastructure =
                  rendered
                  |> List.find (fun (article, _) -> article.permalink = "personal-infrastructure")
                  |> snd

              let developmentEnvironment =
                  rendered |> List.find (fun (article, _) -> article.permalink = "dev-env") |> snd

              Expect.stringContains semanticKernel "Semantic Kernel SDK" "Expected Semantic Kernel content"

              for topic in
                  [ "personal applications"
                    "andymeier.dev"
                    "meiermade.com"
                    "platform-identity/"
                    "platform-infrastructure/"
                    "environments/"
                    "andymeier/"
                    "meiermade/"
                    "agent/"
                    "skills/"
                    "separate repositories"
                    "Google Kubernetes Engine"
                    "Google Workspace"
                    "GKE free tier"
                    "$74.40"
                    "Namespaces, Deployments, and Services"
                    "Cloud Logging"
                    "Cloud Monitoring"
                    "Cloudflare Access"
                    "Define a shared policy"
                    "Protect the HyperDX hostname"
                    "Route the tunnel to the private Service"
                    "new cloudflare.ZeroTrustAccessPolicy"
                    "new cloudflare.ZeroTrustAccessApplication"
                    "new cloudflare.ZeroTrustTunnelCloudflared("
                    "new cloudflare.ZeroTrustTunnelCloudflaredConfig"
                    "policies: [{ id: allowAdmins.id, precedence: 1 }]"
                    "hyperdx.example.com"
                    "audTags: [hyperdx.aud]"
                    "required: true"
                    "hyperdxDeployment.uiServiceUrl"
                    "http_status:404"
                    "&lt;tunnel-id&gt;.cfargotunnel.com"
                    "outbound-only"
                    "Pulumi ESC"
                    "IntelliSense"
                    "Secret Manager"
                    "OpenID Connect"
                    "GitHub Actions"
                    "GitHub CLI"
                    "Benji"
                    "Minnie"
                    "StatefulSet"
                    "OpenTelemetry"
                    "OpenTelemetry Collector"
                    "EventName"
                    "ClickHouse"
                    "ClickStack"
                    "HyperDX"
                    "Cloud SQL PostgreSQL"
                    "cloudflared sidecar"
                    "no cluster-wide default-deny policy"
                    "One telemetry pipeline"
                    "Browser visibility and privacy"
                    "Agents as operators"
                    "Browser telemetry signals"
                    "Credential lifecycles"
                    "Platform tradeoffs"
                    "accTitle: Application and browser telemetry"
                    "accTitle: Reviewed application delivery"
                    "scope=\"col\""
                    "scope=\"row\""
                    "Web Vitals"
                    "default-on analytics with an opt-out"
                    "off in the normal local Watch workflow"
                    "not session replay"
                    "1Password"
                    "30-day application telemetry retention"
                    "strict CORS"
                    "privateClusterConfig"
                    "fn::open::gcp-secrets"
                    "new k8s.apps.v1.Deployment"
                    "mermaid.11.16.0.min.js"
                    "flowchart TB"
                    "accTitle: Personal infrastructure system context"
                    "accTitle: Personal infrastructure runtime"
                    "accTitle: Personal infrastructure deployment"
                    "Personal applications"
                    "strong APIs"
                    "agent-driven workflow"
                    "System context"
                    "Runtime"
                    "Deployment"
                    "same discipline I bring to client infrastructure"
                    "proving ground for new tools and architectural patterns"
                    "experience here informs those decisions"
                    "https://pi.dev/"
                    "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/system-context-dec390cb7efb.webp"
                    "narrowly scoped viewer roles"
                    "Routes website traffic"
                    "Routes agent traffic"
                    "Opens protected interface" ] do
                  Expect.stringContains infrastructure topic $"Expected infrastructure content for {topic}"

              for excludedTopic in
                  [ "Meier Made Platform"
                    "Seq"
                    "Dual-exports"
                    "during migration"
                    "Immutable public assets"
                    "immutable files"
                    "personal-cloud/"
                    "applications/"
                    "&lt;personal-application&gt;"
                    "Auth0"
                    "Dagster"
                    "Airbyte"
                    "Metabase"
                    "Raspberry Pi"
                    "Penpot"
                    "Amazon Web Services"
                    "Redis"
                    "Memorystore" ] do
                  Expect.isFalse
                      (infrastructure.Contains excludedTopic)
                      $"Expected out-of-scope infrastructure content for {excludedTopic} to be removed"

              Expect.isFalse
                  (infrastructure.Contains "subgraph personal")
                  "Expected runtime relationships to terminate at containers rather than a decorative boundary"

              for metaLanguage in
                  [ "the example I will follow through this article"
                    "The runtime view names"
                    "The deployment view makes"
                    "simplest representative workload"
                    "better explanation for the platform"
                    "Identifiers and account details are intentionally omitted"
                    "A VPN would still be appropriate"
                    "The application repository has a deliberately ordinary shape"
                    "Kubernetes often gets a bad reputation"
                    "The point is not to turn infrastructure"
                    "The goal is not to use the fewest technologies" ] do
                  Expect.isFalse
                      (infrastructure.Contains metaLanguage)
                      $"Expected meta language for {metaLanguage} to be removed"

              Expect.isLessThan
                  (infrastructure.IndexOf "How it is organized")
                  (infrastructure.IndexOf "Personal applications and agents")
                  "Expected infrastructure ownership to be explained before individual technologies"

              for applicationDetail in
                  [ "href=\"#example\""
                    "https://github.com/meiermade/andymeier"
                    "source for andymeier.dev"
                    "The site keeps article content in source control"
                    "I keep application code, infrastructure, tests, and workflows together" ] do
                  Expect.isFalse
                      (infrastructure.Contains applicationDetail)
                      $"Expected the andymeier.dev application section to omit {applicationDetail}"

              Expect.isFalse
                  (infrastructure.Contains "https://github.com/meiermade/agent")
                  "Expected the private agent repository not to be linked"

              for tool in
                  [ "Homebrew"
                    "Ghostty"
                    "Starship"
                    "tmux"
                    "GitHub CLI"
                    "1Password CLI"
                    "fnm"
                    "Node.js"
                    "uv"
                    ".NET SDK"
                    "Pi coding agent"
                    "PyCharm"
                    "Rider"
                    "WebStorm"
                    "Docker Desktop"
                    "gcloud"
                    "kubectl"
                    "kubectx"
                    "kubens"
                    "Pulumi"
                    "cloudflared"
                    "Google Workspace CLI"
                    "ripgrep"
                    "jq"
                    "Playwright" ] do
                  Expect.stringContains
                      developmentEnvironment
                      tool
                      $"Expected development environment content for {tool}"

              Expect.isFalse
                  (developmentEnvironment.Contains "Windows Subsystem for Linux")
                  "Expected obsolete Windows setup content to be removed"
          }

          test "finds articles by permalink" {
              Expect.isSome (Catalog.tryFind "personal-infrastructure") "Expected an existing article"
              Expect.isSome (Catalog.tryFind "bpc-157-elbow-research") "Expected the recovery journal to resolve"
              Expect.isNone (Catalog.tryFind "missing") "Expected an unknown permalink not to resolve"
          } ]
