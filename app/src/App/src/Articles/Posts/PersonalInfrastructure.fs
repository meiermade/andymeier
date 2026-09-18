module App.Articles.Posts.PersonalInfrastructure

open App.Articles
open App.Articles.Shared
open App.Common.View
open FSharp.ViewEngine
open System
open type Datastar
open type Html

let private metadata =
    { permalink = "personal-infrastructure"
      title = "Personal Infrastructure"
      summary =
        "How I run my personal and company websites and AI agents on a shared platform with GCP, Kubernetes, Cloudflare, Pulumi, and OpenTelemetry"
      cover = "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/system-context-dec390cb7efb.webp"
      tags = [| "DevOps"; "Pulumi"; "GCP"; "Kubernetes"; "Cloudflare"; "TypeScript" |]
      createdAt = DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero) }

let private subheading label = h3 {
    _class "mt-8"
    text label
}

let private paragraph value = p { text value }

let private inlineCode value = code {
    _class "language-none"
    text value
}

let private codeBlock language value = pre {
    _class $"language-{language}"

    code {
        _class $"language-{language}"
        span { text value }
    }
}

let private link href label = a {
    _href href
    text label
}

let private systemContextDiagram =
    """flowchart TB
    accTitle: Personal infrastructure system context
    accDescr: The operator and visitors use personal applications supported by GitHub Actions, Pulumi Cloud, Google Cloud, Cloudflare, and Google Workspace.

    operator["Operator<br/>(Person)<br/>Builds and operates personal applications"]
    visitors["Visitors<br/>(People)<br/>Use public personal applications"]
    platform["Personal infrastructure<br/>(Software system)<br/>Runs personal and company websites and AI agents"]:::system
    github["GitHub Actions<br/>(External system)<br/>Tests, previews, and deploys reviewed changes"]
    pulumi["Pulumi Cloud and ESC<br/>(External system)<br/>Stores state and composes environments"]
    gcp["Google Cloud<br/>(External system)<br/>Runs compute, storage, messaging, and analytics"]
    cloudflare["Cloudflare<br/>(External system)<br/>Provides DNS, edge policy, Access, and tunnels"]
    workspace["Google Workspace<br/>(External system)<br/>Provides identity for protected applications"]

    operator -->|Commits and reviews changes| github
    operator -->|Operates applications through| cloudflare
    visitors -->|Use public applications through| cloudflare
    github -->|Runs Pulumi programs| pulumi
    pulumi -->|Applies desired state| platform
    cloudflare -->|Routes accepted requests| platform
    cloudflare -->|Checks protected access with| workspace
    platform -->|Runs on| gcp

    classDef system fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private runtimeDiagram =
    """flowchart TB
    accTitle: Personal infrastructure runtime
    accDescr: Visitors and the operator reach andymeier.dev, meiermade.com, and the agents through outbound Cloudflare tunnels. The websites and agents have cloudflared sidecars. Browser telemetry uses a constrained public receiver; application telemetry uses an internal receiver. ClickStack combines ClickHouse storage with the HyperDX interface. Agents keep durable control state in PostgreSQL.

    visitors["Visitors and operator<br/>(People)"]
    cloudflare["Cloudflare<br/>(External system)<br/>Routes traffic and applies Access policy"]
    websites["andymeier.dev and meiermade.com<br/>(Applications)<br/>Each runs with a cloudflared sidecar"]:::primary
    agents["Benji and Minnie<br/>(Applications)<br/>Each runs with a cloudflared sidecar"]:::primary
    collector["OpenTelemetry Collector<br/>(Container)<br/>Separate internal and public receivers"]
    clickhouse[("ClickHouse<br/>(Data store)<br/>Events, logs, traces, and metrics")]
    hyperdx["HyperDX<br/>(Application)<br/>Explore ClickStack telemetry"]
    postgres[("PostgreSQL<br/>(Managed Cloud SQL)<br/>Agent inboxes and task-control state")]
    assets[("Cloud Storage<br/>(Data store)<br/>Public assets")]

    visitors -->|Use public or authorized applications| cloudflare
    cloudflare -->|Routes website traffic through tunnels| websites
    cloudflare -->|Routes agent traffic through tunnels| agents
    cloudflare -->|Rate-limits browser OTLP through a shared tunnel| collector
    cloudflare -->|Opens protected interface through a shared tunnel| hyperdx
    websites -->|Export internal OTLP| collector
    agents -.->|Configured OTLP destination| collector
    collector -->|Stores telemetry| clickhouse
    hyperdx -->|Queries| clickhouse
    agents -->|Persist durable control state| postgres
    websites -->|Reference public assets| assets

    classDef primary fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private deploymentDiagram =
    """flowchart TB
    accTitle: Personal infrastructure deployment
    accDescr: GitHub Actions and Pulumi deploy two website Deployments, separate agent StatefulSets, and ClickStack into a zonal GKE cluster. Each website and agent Pod includes cloudflared. Shared connectors expose HyperDX and browser telemetry. Cloud SQL PostgreSQL sits outside the cluster; ClickHouse, MongoDB, agent workspaces, and the Collector queue use persistent disks.

    github["GitHub Actions<br/>Reviewed delivery"]
    pulumi["Pulumi Cloud and ESC<br/>Desired state and configuration"]
    cloudflare["Cloudflare edge<br/>DNS, Access, and tunnels"]

    subgraph gcp["Google Cloud"]
        direction TB
        registry[("Artifact Registry<br/>Immutable images")]
        subgraph gke["Zonal GKE cluster"]
            direction TB
            websites["andymeier.dev and meiermade.com<br/>Separate Deployments<br/>App + cloudflared per Pod"]:::primary
            agents["Benji and Minnie<br/>Separate StatefulSets<br/>App + cloudflared and workspace disks"]:::primary
            connectors["Shared cloudflared connectors<br/>Platform endpoints"]
            collector["OpenTelemetry Collector<br/>Deployment + persistent queue"]
            hyperdx["HyperDX<br/>Deployment"]
            data[("ClickHouse and MongoDB<br/>Operator-managed persistent workloads")]
            runtime["GKE system components"]
        end
        postgres[("Cloud SQL PostgreSQL<br/>Managed agent control databases")]
        secrets[("Secret Manager<br/>Deployment and application secrets")]
        storage[("Cloud Storage<br/>Public assets")]
        operations[("Cloud Logging and Monitoring<br/>System logs and infrastructure metrics")]

        registry -->|Website images| websites
        registry -->|Agent image| agents
        connectors -->|Public browser OTLP| collector
        connectors -->|Protected interface| hyperdx
        websites -->|Internal OTLP| collector
        agents -.->|Configured OTLP destination| collector
        collector -->|Stores telemetry in ClickHouse| data
        hyperdx -->|Telemetry and application state| data
        agents -->|Durable inboxes and task state| postgres
        websites -->|Reference assets| storage
        runtime -->|System telemetry| operations
    end

    github -->|OIDC and reviewed updates| pulumi
    github -->|Publishes through Pulumi| registry
    pulumi -->|Applies desired state| gcp
    secrets -->|Selected values through ESC| pulumi
    pulumi -->|Configures edge policy| cloudflare
    cloudflare <-->|Website sidecar tunnels| websites
    cloudflare <-->|Agent sidecar tunnels| agents
    cloudflare <-->|Platform tunnels| connectors

    style gcp fill:transparent,stroke:#047857,stroke-width:3px
    style gke fill:transparent,stroke:#059669,stroke-width:2px
    classDef primary fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private architectureDiagram dataAttribute diagram = figure {
    _class
        "not-prose my-8 max-w-full overflow-x-auto rounded-xl border border-gray-200 bg-white p-4 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-700 dark:bg-gray-800 sm:p-6"

    _attr (dataAttribute, "true")
    _attr ("tabindex", "0")
    _dataInit "renderMermaid($el)"

    if dataAttribute <> "data-delivery-flow" then
        figcaption {
            _class "sticky left-0 mb-3 w-fit text-xs text-gray-500 dark:text-gray-400 md:hidden"
            text "Scroll horizontally to see the complete diagram."
        }

    div {
        _class "mermaid article-mermaid"
        text diagram
    }
}

let private systemContext =
    architectureDiagram "data-system-context" systemContextDiagram

let private runtimeView = architectureDiagram "data-container-view" runtimeDiagram

let private deploymentView =
    architectureDiagram "data-deployment-view" deploymentDiagram

let private telemetryView =
    architectureDiagram
        "data-telemetry-flow"
        """flowchart TB
    accTitle: Application and browser telemetry
    accDescr: Browser analytics passes regional policy and visitor choice before reaching a filtered public receiver. Application telemetry uses a separate internal receiver. Both Collector receivers feed ClickHouse, which HyperDX queries.

    browser["Browser analytics"]
    policy["Regional policy<br/>and visitor choice"]
    applications["Application telemetry"]
    subgraph collector["OpenTelemetry Collector"]
        public["Public receiver<br/>Filter and minimize"]
        internal["Internal receiver"]
    end
    clickhouse[("ClickHouse<br/>Events, logs, traces, metrics")]
    hyperdx["HyperDX<br/>Search and correlate"]:::primary

    browser --> policy --> public
    applications --> internal
    public --> clickhouse
    internal --> clickhouse
    clickhouse -->|Queried through| hyperdx

    classDef primary fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private deliveryView =
    architectureDiagram
        "data-delivery-flow"
        """flowchart TB
    accTitle: Reviewed application delivery
    accDescr: A pull request runs tests and a Pulumi preview before review and merge. After merge, CI builds and publishes the image, deploys through Pulumi, waits for readiness, and runs acceptance checks.

    pr["Pull request"]
    checks["Tests + Pulumi preview"]
    review["Review"]
    merge["Merge"]
    deploy["Build + publish image<br/>Pulumi deploy + readiness"]
    acceptance["Browser or API<br/>acceptance checks"]:::primary

    pr --> checks --> review --> merge --> deploy --> acceptance

    classDef primary fill:#059669,stroke:#047857,color:#ffffff,stroke-width:3px"""

let private comparisonTable label columns rows = div {
    _class
        "my-6 overflow-x-auto rounded-lg focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600"

    _attr ("tabindex", "0")
    _role "region"
    _ariaLabel label

    p {
        _class "not-prose sticky left-0 mb-3 w-fit text-xs text-gray-500 dark:text-gray-400 md:hidden"
        text "Scroll horizontally to read the full table."
    }

    table {
        _class "my-0 w-full min-w-[36rem] text-base"

        caption {
            _class "sr-only"
            text label
        }

        thead {
            tr {
                for column in columns do
                    th {
                        _scope "col"
                        text column
                    }
            }
        }

        tbody {
            for label, values in rows do
                tr {
                    th {
                        _scope "row"
                        _class "py-3 pr-4 text-left align-top font-medium text-gray-900 dark:text-gray-100"
                        text label
                    }

                    for value in values do
                        td {
                            _class "py-3 pr-4 align-top"
                            text value
                        }
                }
        }
    }
}

let private organizationTree =
    """platform-identity/       # Cloud workload identities and OIDC trust
platform-infrastructure/ # Shared compute, networking, data, and observability
environments/            # Pulumi ESC configuration
andymeier/               # Personal website and articles
meiermade/               # Company website
agent/                   # Shared runtime for Benji and Minnie
skills/                  # Reusable agent instructions and tools"""

let private gkeExample =
    """const cluster = new gcp.container.Cluster('personal', {
    location: `${region}-b`,
    network: network.id,
    subnetwork: subnet.id,
    removeDefaultNodePool: true,
    ipAllocationPolicy: {
        clusterSecondaryRangeName: 'pods',
        servicesSecondaryRangeName: 'services',
    },
    privateClusterConfig: {
        enablePrivateNodes: true,
        enablePrivateEndpoint: false,
    },
    releaseChannel: { channel: 'REGULAR' },
    workloadIdentityConfig: {
        workloadPool: `${projectId}.svc.id.goog`,
    },
})

new gcp.container.NodePool('personal-primary', {
    cluster: cluster.name,
    autoscaling: { minNodeCount: 1, maxNodeCount: 4 },
    management: { autoRepair: true, autoUpgrade: true },
})"""

let private applicationDeploymentExample =
    """const deployment = new k8s.apps.v1.Deployment('app', {
    metadata: { namespace: config.k8s.namespace },
    spec: {
        replicas: 1,
        selector: { matchLabels: labels },
        template: {
            metadata: { labels },
            spec: {
                securityContext: { runAsNonRoot: true },
                containers: [{
                    name: 'app',
                    image: image.imageRef,
                    resources: {
                        requests: { cpu: '25m', memory: '64Mi' },
                        limits: { cpu: '250m', memory: '256Mi' },
                    },
                    livenessProbe: {
                        httpGet: { path: '/health', port: 5000 },
                    },
                    readinessProbe: {
                        httpGet: { path: '/health', port: 5000 },
                    },
                }],
            },
        },
    },
})

new k8s.core.v1.Service('app', {
    metadata: { namespace: config.k8s.namespace },
    spec: { type: 'ClusterIP', selector: labels, ports: [{ port: 80 }] },
})"""

let private cloudflarePolicyExample =
    """const accountId = config.cloudflareConfig.accountId

const allowAdmins = new cloudflare.ZeroTrustAccessPolicy('allow-admins', {
    accountId,
    name: 'Allow Admins',
    decision: 'allow',
    includes: [{ email: { email: 'admin@example.com' } }],
}, { provider })"""

let private cloudflareApplicationExample =
    """const hostname = 'hyperdx.example.com'

const hyperdx = new cloudflare.ZeroTrustAccessApplication('hyperdx.example.com', {
    accountId,
    name: 'HyperDX',
    domain: hostname,
    type: 'self_hosted',
    allowedIdps: [accessIdentityProvider.googleAccessIdentityProviderId],
    autoRedirectToIdentity: true,
    httpOnlyCookieAttribute: true,
    policies: [{ id: allowAdmins.id, precedence: 1 }],
}, { provider })"""

let private cloudflareTunnelExample =
    """const platformTunnel = new cloudflare.ZeroTrustTunnelCloudflared('platform', {
    accountId,
    name: 'platform',
    configSrc: 'cloudflare',
}, { provider })

new cloudflare.ZeroTrustTunnelCloudflaredConfig('gcp-platform', {
    accountId,
    tunnelId: platformTunnel.id,
    source: 'cloudflare',
    config: {
        ingresses: [{
            hostname,
            // Internal Kubernetes Service, not the connector's localhost.
            service: hyperdxDeployment.uiServiceUrl,
            originRequest: {
                access: {
                    required: true,
                    audTags: [hyperdx.aud],
                    teamName: config.cloudflareConfig.teamName,
                },
            },
        }, {
            service: 'http_status:404',
        }],
    },
}, { provider })"""

let private escExample =
    """values:
  gcpLogin:
    fn::open::gcp-login:
      project: <project-number>
      oidc:
        workloadPoolId: <workload-pool>
        providerId: pulumi
        serviceAccount: <environment-service-account>
        subjectAttributes:
          - currentEnvironment.name

  secrets:
    fn::open::gcp-secrets:
      login: ${gcpLogin}
      access:
        applicationApiKey:
          name: <secret-name>

  environmentVariables:
    GOOGLE_OAUTH_ACCESS_TOKEN: ${gcpLogin.accessToken}

  pulumiConfig:
    application:apiKey: ${secrets.applicationApiKey}"""

let private githubWorkflowExample =
    """permissions:
  contents: read
  id-token: write

steps:
  - uses: actions/checkout@v7
  - name: Authenticate with Pulumi
    uses: pulumi/auth-actions@v2
    with:
      organization: <organization>
      requested-token-type: <scoped-token-type>
  - name: Preview or update
    uses: pulumi/actions@v7
    with:
      work-dir: ./pulumi
      stack-name: <organization>/<project>/prod
      command: <preview-or-up>"""

let private introduction =
    [ p {
          text
              "I run my personal website, company website, and long-lived AI agents on a shared cloud platform. andymeier.dev publishes my articles, meiermade.com presents my consulting business, and Benji and Minnie help with recurring work. Google Cloud provides the foundation, Kubernetes gives each workload a common deployment API, Cloudflare handles traffic and access, Pulumi defines the infrastructure, and GitHub Actions delivers changes."
      }
      p {
          text
              "I operate it with the same discipline I bring to client infrastructure: changes are defined in code, previewed, and reviewed; identities are narrowly scoped; secrets stay out of repositories; and deployments are observable and reproducible. Because I own the cost and blast radius, it also serves as a proving ground for new tools and architectural patterns before I apply what I learn to client work."
      }
      p {
          text
              "I favor a small, coherent set of tools with strong APIs. In an agent-driven workflow, that lets me and my agents build, inspect, and diagnose the same systems programmatically. Every client environment has its own scale, risk, compliance, and operational requirements, so experience here informs those decisions rather than becoming a default blueprint."
      } ]

let private sections =
    [ ArticlePage.section
          "architecture"
          "Architecture at a glance"
          [ subheading "System context"
            paragraph
                "Cloudflare fronts every public and protected application, while GitHub Actions and Pulumi deliver reviewed changes to Google Cloud. Google Workspace provides my identity for protected applications."
            systemContext
            subheading "Runtime"
            paragraph
                "andymeier.dev and meiermade.com are public websites; Benji and Minnie are long-lived AI agents with protected interfaces and authenticated webhook routes. A shared OpenTelemetry Collector separates internal application telemetry from constrained public browser telemetry. ClickStack combines ClickHouse storage with HyperDX for exploration. The agents use managed PostgreSQL for durable inboxes and task-control state."
            runtimeView
            subheading "Deployment"
            paragraph
                "Application images live in Artifact Registry. The websites, agents, Collector, HyperDX, ClickHouse, and MongoDB run in one zonal GKE cluster. Each website and agent Pod includes its own cloudflared sidecar; shared connectors expose platform endpoints. Cloud SQL PostgreSQL, Secret Manager, Cloud Storage, Cloud Logging, and Cloud Monitoring sit outside the cluster as managed Google Cloud services."
            deploymentView ]
      ArticlePage.section
          "organization"
          "How it is organized"
          [ paragraph
                "These are separate repositories in the Meier Made GitHub organization, not directories in a single application. I organize the infrastructure in dependency order: workload identity, shared infrastructure, application environments, and application-owned deployments."
            codeBlock "none" organizationTree
            p {
                text "Each resource has one owner. The "
                inlineCode "platform-infrastructure"
                text " repository owns shared capabilities. The "
                inlineCode "andymeier"
                text ", "
                inlineCode "meiermade"
                text ", and "
                inlineCode "agent"
                text " repositories each contain "
                inlineCode "app/"
                text " for application code and "
                inlineCode "pulumi/"
                text " for their deployments, routes, and access policies. The "
                inlineCode "environments"
                text " repository composes stack outputs and credentials through ESC. The "
                inlineCode "skills"

                text
                    " repository supplies reusable instructions and command tools without owning the services they access."
            }
            paragraph
                "Namespaces and Kubernetes RBAC define workload ownership and deployment permissions. They are not automatic network isolation: the cluster enables NetworkPolicy enforcement, but applications must declare the traffic restrictions they need, and there is no cluster-wide default-deny policy. Workload Identity binds Kubernetes service accounts to authorized Google service accounts without distributing cloud keys." ]
      ArticlePage.section
          "applications"
          "Personal applications and agents"
          [ paragraph
                "andymeier.dev and meiermade.com are stateless F# web applications. The first is my personal website and technical blog; the second is my company website, with services, projects, team information, and public policies. Both run behind private origins and share deployment, networking, and observability conventions."
            p {
                text
                    "Benji and Minnie are my two long-running AI agents. I use them for coding, scheduled routines, email and messaging, and task follow-up. They share one runtime but deploy as separate single-replica StatefulSets, with separate configuration, Kubernetes service accounts, persistent workspaces, hostnames, Cloudflare policies, and "

                link "https://pi.dev/" "Pi coding agent"

                text
                    " sessions. Managed Cloud SQL PostgreSQL holds each agent's durable communication inbox and task-control state, while persistent volumes hold workspaces and session files."
            }
            paragraph
                "The agents need persistent processes, protected endpoints, background work, workload identity, and durable state. Kubernetes gives them and the stateless websites one operational model without forcing them into the same architecture." ]
      ArticlePage.section
          "gcp"
          "Why Google Cloud"
          [ paragraph
                "Google Cloud fits this environment because Google Workspace, IAM, GKE, Artifact Registry, Secret Manager, Cloud Logging, and Cloud Monitoring share a coherent identity and operations model. Its APIs and command-line tools are consistent, and Google Kubernetes Engine (GKE) is the managed Kubernetes service I know best and prefer."
            p {
                text
                    "The economics of a small zonal cluster are excellent. Google charges a cluster management fee, but the "

                link "https://cloud.google.com/kubernetes-engine/pricing" "GKE free tier"

                text
                    " provides $74.40 in monthly credits per billing account, offsetting the management fee for one zonal Standard or Autopilot cluster. Worker nodes, disks, networking, and usage-based services remain billable, but the managed control plane adds no incremental fee within that allowance."
            }
            paragraph
                "I use a zonal Standard cluster to control node pools, pack small workloads efficiently, and avoid paying for multi-zone availability I do not need. Auto-repair, auto-upgrade, a regular release channel, and reproducible configuration handle much of the maintenance."
            paragraph "This TypeScript is an abridged version of my zonal GKE configuration:"
            codeBlock "typescript" gkeExample ]
      ArticlePage.section
          "kubernetes"
          "Kubernetes without platform engineering"
          [ paragraph
                "I start with Namespaces, Deployments, and Services, then add StatefulSets and persistent volumes for agents, Jobs for bounded work, and operators for the databases inside the cluster. Deployments create and replace Pods, Services give them stable private addresses, and Namespaces organize ownership. The websites stay simple even when another workload needs more machinery."
            p {
                text "That small subset still provides a useful "
                link "https://kubernetes.io/docs/concepts/overview/kubernetes-api/" "programmatic API"

                text
                    ". A Deployment declares its image, replicas, probes, resources, and security context. Kubernetes reconciles the workload, restarts failed containers, and waits for readiness during a rollout. That gives me repeatable deployments without custom process managers or remote-shell scripts."
            }
            paragraph
                "The same tools list workloads, inspect events, stream logs, restart rollouts, and forward private ports for every application. I configure GKE to send system-component logs to Cloud Logging and infrastructure metrics to Cloud Monitoring. Routine application stdout and stderr are not also collected into Cloud Logging; application observability uses OpenTelemetry and ClickStack."
            paragraph "This is an abridged application Deployment and Service:"
            codeBlock "typescript" applicationDeploymentExample ]
      ArticlePage.section
          "cloudflare"
          "Cloudflare for networking and access"
          [ p {
                text "All application origins stay on the private network. A "

                link
                    "https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/"
                    "Cloudflare Tunnel"

                text
                    " connector runs in Kubernetes and creates an outbound-only connection to Cloudflare. The cluster does not need a public application load balancer. For each website and agent, cloudflared runs as a sidecar in the application Pod and forwards to the app over localhost; Services remain private ClusterIP addresses. Shared platform connectors route to internal Services for endpoints such as HyperDX and the browser Collector."
            }
            p {
                text "For a private application, "

                link
                    "https://developers.cloudflare.com/cloudflare-one/setup/secure-private-apps/private-web-app/"
                    "Cloudflare Access"

                text
                    " checks identity before forwarding the request to the origin. Access applications protect hostnames and attach reusable policies by ID, while Google Workspace supplies the human identity. The tunnel carries traffic; the Access application determines who may use it. I define both through Pulumi."
            }
            p {
                text "HyperDX is a concrete example. These abridged TypeScript examples adapted from "
                inlineCode "platform-infrastructure"

                text
                    " show the shared policy, protected application, and tunnel route. Provider setup and imports are omitted; configuration and referenced resource outputs come from the surrounding modules."
            }
            subheading "Define a shared policy"
            paragraph "I define the administrator policy once, then reuse it across protected applications:"
            codeBlock "typescript" cloudflarePolicyExample
            subheading "Protect the HyperDX hostname"
            paragraph
                "The HyperDX Access application selects the shared Google Workspace identity provider and attaches the administrator policy by ID:"
            codeBlock "typescript" cloudflareApplicationExample
            p {
                text
                    "The full application also attaches the reusable Pi, Benji, and Minnie service-token policies. Benji and Minnie additionally require the platform NAT address. The "

                inlineCode "platform-infrastructure"

                text
                    " repository exports shared policy and identity-provider IDs; product repositories consume them through ESC rather than redefining the shared resources."
            }
            subheading "Route the tunnel to the private Service"
            paragraph
                "The shared platform connector runs as a standalone Kubernetes Deployment, unlike the application sidecars. This excerpt keeps only its HyperDX route. The connector requires an Access token for the application's audience before forwarding to the internal Service:"
            codeBlock "typescript" cloudflareTunnelExample
            p {
                text "A proxied CNAME points the hostname to "
                inlineCode "<tunnel-id>.cfargotunnel.com"

                text
                    ". Pulumi retrieves the tunnel token and places it in a Kubernetes Secret consumed by cloudflared. The final 404 rule rejects unmatched hostnames. Public websites and the browser telemetry endpoint do not require an Access login; protection is chosen per application, not implied by using a tunnel."
            } ]
      ArticlePage.section
          "observability"
          "OpenTelemetry and ClickStack"
          [ subheading "One telemetry pipeline"
            p {
                text "The websites send standard OTLP to an "
                link "https://opentelemetry.io/docs/collector/" "OpenTelemetry Collector"
                text ". Separate receivers keep internal application telemetry apart from public browser input. "

                link
                    "https://clickhouse.com/docs/use-cases/observability/clickstack/overview"
                    "ClickHouse and ClickStack"

                text " provide storage and exploration, with HyperDX as the search interface."
            }
            telemetryView
            paragraph
                "Named browser and business occurrences use OpenTelemetry EventRecords: the top-level EventName distinguishes them from diagnostic logs. Semantic views separate those records in ClickHouse. The Collector is configured for 30-day application telemetry retention and a persistent delivery queue; ClickHouse's own diagnostic logs have shorter retention."
            paragraph
                "The agent environments also select the internal Collector. Configuration alone does not prove delivery: exporter validation requires fresh records, and correlation depends on propagated trace context. Cloud Logging and Cloud Monitoring supply the separate system-level view."
            subheading "Browser visibility and privacy"
            comparisonTable
                "Browser telemetry signals"
                [ "Signal"; "What it helps explain" ]
                [ "Navigation and engagement",
                  [ "What people read and explore: article opens, completion, and outbound links on andymeier.dev; service/project views and contact/scheduling clicks on meiermade.com." ]
                  "Fetch traces",
                  [ "Request timing and failures, with same-origin trace-context propagation where supported." ]
                  "JavaScript errors", [ "Client-side problems that server logs alone cannot explain." ]
                  "Web Vitals", [ "Page experience, without backfilling a page load from before acceptance." ] ]
            paragraph
                "A tab-scoped session ID links browser records; this is not session replay. Opt-in regions and unknown locations require acceptance. Other recognized locations use default-on analytics with an opt-out. A saved refusal applies in either mode, and visitors can withdraw through analytics settings. Collection is off in the normal local Watch workflow, independently of server observability."
            paragraph
                "The public path applies strict CORS, request-size and Cloudflare rate limits, event-name validation, and attribute allowlists. The Collector clears log bodies and span status messages; navigation URLs become paths and campaign attribution is constrained. Browser telemetry remains untrusted input, not proof of an authenticated action."
            subheading "Agents as operators"
            paragraph
                "Reusable skills let me and the agents search HyperDX, follow trace IDs, and run bounded read-only ClickHouse queries. Their Kubernetes identities can inspect workloads, events, and logs; their shared Google runtime principal has narrowly scoped viewer roles for cluster, logging, and monitoring context, not deployment authority."
            comparisonTable
                "Credential lifecycles"
                [ "Purpose"; "Credential path"; "Boundary" ]
                [ "Deployment and application configuration",
                  [ "Secret Manager → Pulumi ESC → deployment"
                    "Selected secrets and short-lived cloud identity for the authorized environment." ]
                  "Agent service commands",
                  [ "Caller-scoped 1Password vault → selected command → protected service"
                    "Resolve only the credentials needed by that subprocess; no new deployment secret for each tool." ] ]
            paragraph
                "Cloudflare Access authenticates the perimeter. A HyperDX API key or ClickHouse database credential separately authorizes backend access. Reaching the service is not permission to read its data." ]
      ArticlePage.section
          "pulumi"
          "Pulumi and environments"
          [ p {
                text "I define infrastructure with "
                link "https://www.pulumi.com/docs/iac/languages-sdks/javascript/" "Pulumi and TypeScript"

                text
                    " to reuse policies and resource shapes with normal language and refactoring tools. Provider types and IntelliSense expose available properties while I write and review changes, and strong types provide useful evidence even when AI helps with discovery."
            }
            p {
                text "Pulumi ESC composes stack outputs, configuration, and selected secrets for each environment. Its "
                link "https://www.pulumi.com/docs/esc/guides/pulumi-iac/" "GCP login provider"

                text
                    " exchanges OpenID Connect identity for a temporary Google Cloud token. A dedicated service account can read only the authorized secrets; the repository receives no service-account key."
            }
            paragraph
                "I keep resource modules small and ownership explicit. Pulumi calculates previews and records state; ESC resolves environment values when opened. This abridged configuration shows the login, secret lookup, and projection into Pulumi:"
            codeBlock "yaml" escExample ]
      ArticlePage.section
          "github"
          "GitHub for delivery"
          [ paragraph
                "The pull request is the unit of change. Checks and review happen before merge; CI then builds the image, deploys through Pulumi, waits for readiness, and tests the resulting application."
            deliveryView
            p {
                text "GitHub Actions requests an OIDC token, and "

                link "https://www.pulumi.com/docs/iac/guides/continuous-delivery/github-actions/" "Pulumi exchanges it"

                text
                    " for short-lived, scoped access. ESC obtains a separate temporary Google Cloud identity, so trust follows the repository and environment rather than stored access tokens or cloud keys."
            }
            paragraph "The core deployment workflow is small:"
            codeBlock "yaml" githubWorkflowExample
            p {
                text "Locally, the "
                link "https://cli.github.com/manual/" "GitHub CLI"

                text
                    " and GitHub API give me and the agents the same pull requests, diffs, and check results. Production changes follow that review path rather than an unreviewed update from a workstation."
            } ]
      ArticlePage.section
          "tradeoffs"
          "Intentional tradeoffs"
          [ paragraph
                "A static host would be simpler for one website. I accept more moving parts because several websites and agents reuse them. The important choices are explicit:"
            comparisonTable
                "Platform tradeoffs"
                [ "Choice"; "Benefit"; "Cost accepted" ]
                [ "Zonal GKE",
                  [ "Managed Kubernetes with a lower baseline than multi-zone redundancy."
                    "A zone-level incident can interrupt every workload in the cluster." ]
                  "Shared infrastructure",
                  [ "Reuse private ingress, deployment, identity, and observability conventions."
                    "Shared capacity and a common failure boundary need attention." ]
                  "Self-hosted ClickStack",
                  [ "One queryable store for application diagnostics and named events."
                    "Maintain ClickHouse, MongoDB, persistent storage, queue capacity, and retention." ]
                  "Provider dependencies",
                  [ "Google Cloud, Cloudflare, Pulumi, and GitHub remove operational work."
                    "Replacing them takes real migration work, despite portable containers and OTLP." ] ]
            paragraph
                "The value is not just that I can deploy another application. It is that I and my agents can use the same APIs, scoped tools, and telemetry to understand what it is doing."
            script {
                _src (Asset.fingerprinted "/scripts/mermaid.11.16.0.min.js")
                _onload "window.renderMermaid?.(document)"
            }
            script {
                js
                    """
window.renderMermaid = function(el) {
    const render = async () => {
        if (!window.mermaid) return;
        const theme = document.documentElement.classList.contains('dark') ? 'dark' : 'neutral';
        const candidates = el?.matches?.('.mermaid') ? [el] : Array.from(el?.querySelectorAll?.('.mermaid') ?? []);
        const nodes = candidates.filter(node => node.isConnected && (node.dataset.mermaidTheme !== theme || !node.querySelector('svg')));
        if (!nodes.length) return;
        for (const node of nodes) {
            node.dataset.mermaidSource = node.dataset.mermaidSource || node.textContent.trim();
            node.textContent = node.dataset.mermaidSource;
            node.removeAttribute('data-processed');
        }
        window.mermaid.initialize({ startOnLoad: false, theme, securityLevel: 'strict' });
        await window.mermaid.run({ nodes });
        for (const node of nodes) {
            node.dataset.mermaidTheme = theme;
            const scroller = node.parentElement;
            if (scroller && scroller.scrollWidth > scroller.clientWidth && scroller.scrollLeft === 0)
                scroller.scrollLeft = (scroller.scrollWidth - scroller.clientWidth) / 2;
        }
    };
    // Figure initialization and page restoration can coincide; never reset a diagram during another render.
    window.mermaidRenderQueue = (window.mermaidRenderQueue || Promise.resolve()).then(render, render);
    return window.mermaidRenderQueue;
};
void window.renderMermaid(document);
"""
            } ]

      ]

let article =
    Article.create metadata (ArticlePage.primary metadata introduction sections)
