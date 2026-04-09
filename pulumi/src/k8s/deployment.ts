import * as k8s from '@pulumi/kubernetes'
import { provider } from './provider'
import * as image from '../docker/image'
import * as config from '../config'
import * as tunnel from '../cloudflare/tunnel'

let appSecret = new k8s.core.v1.Secret('app', {
    metadata: {
        name: 'app',
        namespace: config.k8sConfig.namespace
    },
    immutable: true,
    stringData: {
        ASPNETCORE_ENVIRONMENT: 'Production',
        SERVER_URL: 'http://0.0.0.0:5000',
        SEQ_ENDPOINT: config.seqConfig.endpoint,
        SQLITE_PATH: '/data/app.db',
        NOTION_ARTICLES_DATABASE_ID: config.notionConfig.articlesDatabaseId,
        NOTION_API_KEY: config.notionConfig.apiKey,
    }
}, { provider })

const cloudflaredSecret = new k8s.core.v1.Secret('cloudflared', {
    metadata: {
        name: 'cloudflared',
        namespace: config.k8sConfig.namespace
    },
    stringData: {
        TUNNEL_TOKEN: tunnel.tunnelToken,
        TUNNEL_METRICS: '0.0.0.0:2000'
    }
}, { provider })

const labels = { 'app.kubernetes.io/name': 'app' }

const podSecurityContext: k8s.types.input.core.v1.PodSecurityContext = {
    runAsNonRoot: true,
    seccompProfile: {
        type: 'RuntimeDefault'
    }
}

const containerSecurityContext: k8s.types.input.core.v1.SecurityContext = {
    allowPrivilegeEscalation: false,
    capabilities: {
        drop: ['ALL']
    }
}

const deployment = new k8s.apps.v1.Deployment('app', {
    metadata: {
        name: 'app',
        namespace: config.k8sConfig.namespace
    },
    spec: {
        replicas: 1,
        selector: { matchLabels: labels },
        template: {
            metadata: { labels: labels },
            spec: {
                securityContext: podSecurityContext,
                containers: [
                    {
                        name: 'app',
                        image: image.imageRef,
                        securityContext: containerSecurityContext,
                        imagePullPolicy: 'IfNotPresent',
                        envFrom: [{ secretRef: { name: appSecret.metadata.name } }],
                        resources: {
                            requests: { cpu: '25m', memory: '64Mi' },
                            limits: { cpu: '250m', memory: '256Mi' },
                        },
                        volumeMounts: [{ name: 'app-data', mountPath: '/data' }],
                        livenessProbe: {
                            httpGet: {
                                path: '/health',
                                port: 5000
                            },
                            initialDelaySeconds: 5
                        },
                        readinessProbe: {
                            httpGet: {
                                path: '/health',
                                port: 5000
                            },
                            initialDelaySeconds: 5
                        }
                    },
                    {
                        name: 'cloudflared',
                        image: `cloudflare/cloudflared:${config.cloudflareConfig.cloudflaredVersion}`,
                        securityContext: containerSecurityContext,
                        args: [
                            'tunnel',
                            '--no-autoupdate',
                            'run'
                        ],
                        envFrom: [{ secretRef: { name: cloudflaredSecret.metadata.name } }],
                        resources: {
                            requests: { cpu: '10m', memory: '32Mi' },
                            limits: { cpu: '100m', memory: '128Mi' },
                        },
                        livenessProbe: {
                            httpGet: { path: '/ready', port: 2000 },
                            failureThreshold: 1,
                            initialDelaySeconds: 10,
                            periodSeconds: 10
                        }
                    }
                ],
                volumes: [
                    {
                        name: 'app-data',
                        emptyDir: {}
                    }
                ]
            }
        }
    }
}, { provider, dependsOn: [appSecret, cloudflaredSecret] })

new k8s.core.v1.Service('app', {
    metadata: {
        name: 'app',
        namespace: config.k8sConfig.namespace
    },
    spec: {
        type: 'ClusterIP',
        selector: labels,
        ports: [{
            name: 'http',
            port: 80,
            targetPort: 5000
        }]
    }
}, { provider, dependsOn: deployment })
