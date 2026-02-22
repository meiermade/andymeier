import * as cloudflare from '@pulumi/cloudflare'
import { andrewmeierTunnel } from './tunnel'
import { zone } from './zone'
import { provider } from './provider'
import * as config from '../config'

export const andrewmeier = new cloudflare.Record(config.domain, {
    name: '@',
    zoneId: zone.id,
    type: 'CNAME',
    content: andrewmeierTunnel.cname,
    proxied: true
}, { provider })
