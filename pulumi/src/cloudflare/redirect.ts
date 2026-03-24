import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import { andrewmeierZone, meiermadeZone } from './zone'
import * as config from '../config'

// andrewmeier.dev -> andymeier.dev
new cloudflare.Ruleset(`${config.identifier}-andrewmeier-redirect`, {
    zoneId: andrewmeierZone.id,
    name: 'Redirect andrewmeier.dev to andymeier.dev',
    kind: 'zone',
    phase: 'http_request_dynamic_redirect',
    rules: [{
        ref: 'andrewmeier_to_andymeier',
        description: 'Redirect andrewmeier.dev and www.andrewmeier.dev to andymeier.dev preserving path',
        enabled: true,
        expression: '(http.host eq "andrewmeier.dev") or (http.host eq "www.andrewmeier.dev")',
        action: 'redirect',
        actionParameters: {
            fromValue: {
                statusCode: 301,
                preserveQueryString: true,
                targetUrl: {
                    expression: 'concat("https://andymeier.dev", http.request.uri.path)'
                }
            }
        }
    }]
}, { provider })

// meiermade.com -> andymeier.dev/services
new cloudflare.Ruleset(`${config.identifier}-meiermade-redirect`, {
    zoneId: meiermadeZone.id,
    name: 'Redirect meiermade.com to andymeier.dev/services',
    kind: 'zone',
    phase: 'http_request_dynamic_redirect',
    rules: [{
        ref: 'meiermade_to_andymeier_services',
        description: 'Redirect meiermade.com and www.meiermade.com to services page',
        enabled: true,
        expression: '(http.host eq "meiermade.com") or (http.host eq "www.meiermade.com")',
        action: 'redirect',
        actionParameters: {
            fromValue: {
                statusCode: 301,
                preserveQueryString: true,
                targetUrl: {
                    value: 'https://andymeier.dev/services'
                }
            }
        }
    }]
}, { provider })
