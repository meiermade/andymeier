import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import * as config from '../config'

export const zone = cloudflare.getZoneOutput({
    accountId: config.cloudflareConfig.accountId,
    name: config.cloudflareConfig.zoneName
}, { provider })
