import * as cloudflare from '@pulumi/cloudflare'
import * as pulumi from '@pulumi/pulumi'
import { provider } from './provider'
import * as config from '../config'

export const andrewmeierTunnel = new cloudflare.ZeroTrustTunnelCloudflared(config.identifier, {
    accountId: config.cloudflareConfig.accountId,
    name: config.identifier,
    secret: config.cloudflareConfig.tunnelSecret.apply(s => Buffer.from(s).toString('base64')),
    configSrc: 'cloudflare'
}, { provider })

new cloudflare.ZeroTrustTunnelCloudflaredConfig(config.identifier, {
    accountId: config.cloudflareConfig.accountId,
    tunnelId: andrewmeierTunnel.id,
    config: {
        ingressRules: [
            {
                hostname: config.domain,
                service: pulumi.interpolate `http://app.${config.andrewmeierProdNamespace}`
            },
            {
                service: 'http_status:404'
            }
        ],
    }
}, { provider })
