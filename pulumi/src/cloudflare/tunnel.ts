import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import * as config from '../config'

export const andymeierTunnel = new cloudflare.ZeroTrustTunnelCloudflared(config.identifier, {
    accountId: config.cloudflareConfig.accountId,
    name: config.identifier,
    configSrc: 'cloudflare'
}, { provider, deleteBeforeReplace: true })

new cloudflare.ZeroTrustTunnelCloudflaredConfig(config.identifier, {
    accountId: config.cloudflareConfig.accountId,
    tunnelId: andymeierTunnel.id,
    source: 'cloudflare',
    config: {
        ingresses: [
            {
                hostname: 'andymeier.dev',
                service: 'http://localhost:5000'
            },
            {
                service: 'http_status:404'
            }
        ]
    }
}, { provider })

export const tunnelHostname = andymeierTunnel.id.apply(id => `${id}.cfargotunnel.com`)

const tunnelTokenRes = cloudflare.getZeroTrustTunnelCloudflaredTokenOutput({
    accountId: config.cloudflareConfig.accountId,
    tunnelId: andymeierTunnel.id
}, { provider })

export const tunnelToken = tunnelTokenRes.token
