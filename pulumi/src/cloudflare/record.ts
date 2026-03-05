import * as cloudflare from '@pulumi/cloudflare'
import { tunnelHostname } from './tunnel'
import { provider } from './provider'
import * as config from '../config'

const zone = cloudflare.getZoneOutput({
    filter: {
        account: {
            id: config.cloudflareConfig.accountId
        },
        name: 'andymeier.dev'
    }
}, { provider })

export const andymeier = new cloudflare.DnsRecord(config.identifier, {
    name: '@',
    zoneId: zone.id,
    type: 'CNAME',
    content: tunnelHostname,
    proxied: true,
    ttl: 1
}, { provider })

// DNS records for redirect domains — proxied A records pointing to a dummy IP
// so Cloudflare can intercept requests and apply redirect rulesets.

const andrewmeierZone = cloudflare.getZoneOutput({
    filter: {
        account: {
            id: config.cloudflareConfig.accountId
        },
        name: 'andrewmeier.dev'
    }
}, { provider })

new cloudflare.DnsRecord(`${config.identifier}-andrewmeier-root`, {
    name: '@',
    zoneId: andrewmeierZone.id,
    type: 'A',
    content: '192.0.2.1',
    proxied: true,
    ttl: 1
}, { provider })

new cloudflare.DnsRecord(`${config.identifier}-andrewmeier-www`, {
    name: 'www',
    zoneId: andrewmeierZone.id,
    type: 'A',
    content: '192.0.2.1',
    proxied: true,
    ttl: 1
}, { provider })

const meiermadeZone = cloudflare.getZoneOutput({
    filter: {
        account: {
            id: config.cloudflareConfig.accountId
        },
        name: 'meiermade.com'
    }
}, { provider })

new cloudflare.DnsRecord(`${config.identifier}-meiermade-root`, {
    name: '@',
    zoneId: meiermadeZone.id,
    type: 'A',
    content: '192.0.2.1',
    proxied: true,
    ttl: 1
}, { provider })

new cloudflare.DnsRecord(`${config.identifier}-meiermade-www`, {
    name: 'www',
    zoneId: meiermadeZone.id,
    type: 'A',
    content: '192.0.2.1',
    proxied: true,
    ttl: 1
}, { provider })
