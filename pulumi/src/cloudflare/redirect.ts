import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import * as config from '../config'

const meiermadeZone = cloudflare.getZoneOutput({
    filter: {
        account: {
            id: config.cloudflareConfig.accountId
        },
        name: 'meiermade.com'
    }
}, { provider })

new cloudflare.Ruleset(`${config.identifier}-meiermade-redirect`, {
    zoneId: meiermadeZone.id,
    name: 'Redirect meiermade.com to andrewmeier.dev/services',
    kind: 'zone',
    phase: 'http_request_dynamic_redirect',
    rules: [{
        ref: 'meiermade_to_andrewmeier_services',
        description: 'Redirect meiermade.com and www.meiermade.com to services page',
        enabled: true,
        expression: '(http.host eq "meiermade.com") or (http.host eq "www.meiermade.com")',
        action: 'redirect',
        actionParameters: {
            fromValue: {
                statusCode: 301,
                preserveQueryString: true,
                targetUrl: {
                    value: 'https://andrewmeier.dev/services'
                }
            }
        }
    }]
}, { provider })
