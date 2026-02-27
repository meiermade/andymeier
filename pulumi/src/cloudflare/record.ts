import * as cloudflare from '@pulumi/cloudflare'
import { tunnelHostname } from './tunnel'
import { provider } from './provider'
import * as config from '../config'

const zone = cloudflare.getZoneOutput({
    filter: {
        account: {
            id: config.cloudflareConfig.accountId
        },
        name: 'andrewmeier.dev'
    }
}, { provider })

export const andrewmeier = new cloudflare.DnsRecord(config.identifier, {
    name: '@',
    zoneId: zone.id,
    type: 'CNAME',
    content: tunnelHostname,
    proxied: true,
    ttl: 1
}, { provider })
