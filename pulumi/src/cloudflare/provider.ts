import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'

export const provider = new cloudflare.Provider('default', {
    apiToken: config.cloudflareConfig.apiToken
})
