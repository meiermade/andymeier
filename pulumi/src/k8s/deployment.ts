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
        SEQ_API_KEY: config.seqConfig.apiKey,
        SQLITE_PATH: '/data/app.db',
        NOTION_ARTICLES_DATABASE_ID: config.notionConfig.articlesDatabaseId,
        NOTION_TOKEN: config.notionConfig.token,
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
                containers: [
                    {
                        name: 'app',
                        image: image.imageName,
                        imagePullPolicy: 'IfNotPresent',
                        envFrom: [{ secretRef: { name: appSecret.metadata.name } }],
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
                        args: [
                            'tunnel',
                            '--no-autoupdate',
                            'run'
                        ],
                        envFrom: [{ secretRef: { name: cloudflaredSecret.metadata.name } }],
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
