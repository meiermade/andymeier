import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import * as config from '../config'

export const andrewmeierTunnel = new cloudflare.ZeroTrustTunnelCloudflared(config.identifier, {
    accountId: config.cloudflareConfig.accountId,
    name: config.identifier,
    configSrc: 'cloudflare'
}, { provider, deleteBeforeReplace: true })

new cloudflare.ZeroTrustTunnelCloudflaredConfig(config.identifier, {
    accountId: config.cloudflareConfig.accountId,
    tunnelId: andrewmeierTunnel.id,
    source: 'cloudflare',
    config: {
        ingresses: [
            {
                hostname: config.cloudflareConfig.zoneName,
                service: `http://app.${config.k8sConfig.namespace}.svc.cluster.local:80`
            },
            {
                service: 'http_status:404'
            }
        ]
    }
}, { provider })

export const tunnelHostname = andrewmeierTunnel.id.apply(id => `${id}.cfargotunnel.com`)

const tunnelTokenRes = cloudflare.getZeroTrustTunnelCloudflaredTokenOutput({
    accountId: config.cloudflareConfig.accountId,
    tunnelId: andrewmeierTunnel.id
}, { provider })

export const tunnelToken = tunnelTokenRes.token
