import * as cloudflare from '@pulumi/cloudflare'
import { tunnelHostname } from './tunnel'
import { zone } from './zone'
import { provider } from './provider'
import * as config from '../config'

export const andrewmeier = new cloudflare.Record(config.identifier, {
    name: '@',
    zoneId: zone.id,
    type: 'CNAME',
    content: tunnelHostname,
    proxied: true
}, { provider })
