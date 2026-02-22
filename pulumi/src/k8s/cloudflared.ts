import * as k8s from '@pulumi/kubernetes'
import { provider } from './provider'
import * as tunnel from '../cloudflare/tunnel'
import * as config from '../config'

const identifier = 'cloudflared'

const secret = new k8s.core.v1.Secret(identifier, {
    metadata: { name:identifier, namespace: config.andrewmeierProdNamespace },
    stringData: {
        TUNNEL_TOKEN: tunnel.andrewmeierTunnel.tunnelToken,
        TUNNEL_METRICS: '0.0.0.0:2000'
    }
}, { provider })

const labels = { 'app.kubernetes.io/name': identifier }

new k8s.apps.v1.Deployment(identifier, {
    metadata: {
        name: identifier,
        namespace: config.andrewmeierProdNamespace
    },
    spec: {
        replicas: 1,
        selector: { matchLabels: labels },
        template: {
            metadata: { labels: labels },
            spec: {
                containers: [{
                    name: identifier,
                    image: `cloudflare/cloudflared:${config.cloudflareConfig.cloudflaredVersion}`,
                    args: [
                        'tunnel',
                        'run'
                    ],
                    envFrom: [{ secretRef: { name: secret.metadata.name } }],
                    livenessProbe: {
                        httpGet: { path: '/ready', port: 2000 },
                        failureThreshold: 1,
                        initialDelaySeconds: 10,
                        periodSeconds: 10
                    }
                }]
            }            
        }
    }
}, { provider })
