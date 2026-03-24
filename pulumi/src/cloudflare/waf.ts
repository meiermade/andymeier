import * as cloudflare from '@pulumi/cloudflare'
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

const expression = [
    '(http.request.uri.path contains "/.env")',
    '(http.request.uri.path contains "/.git")',
    '(http.request.uri.path contains "/wp-")',
    '(http.request.uri.path contains "/wordpress")',
    '(http.request.uri.path contains "/xmlrpc")',
    '(http.request.uri.path contains "/phpMyAdmin")',
    '(http.request.uri.path contains "/phpmyadmin")',
    '(http.request.uri.path contains "/pma")',
    '(http.request.uri.path contains "/cgi-bin")',
    '(http.request.uri.path contains "/actuator")',
    '(http.request.uri.path contains "/solr")',
    '(http.request.uri.path contains "/telescope")',
    '(http.request.uri.path contains "/vendor")',
    '(http.request.uri.path contains ".php")',
    '(http.request.uri.path contains ".asp")',
    '(http.request.uri.path contains ".jsp")',
    '(http.request.uri.path contains ".cgi")',
].join(' or ')

new cloudflare.Ruleset(`${config.identifier}-waf`, {
    zoneId: zone.id,
    name: 'Block vulnerability scanners',
    kind: 'zone',
    phase: 'http_request_firewall_custom',
    rules: [{
        ref: 'block_scan_probes',
        description: 'Block common vulnerability scanner paths and file extensions',
        enabled: true,
        expression,
        action: 'block',
    }]
}, { provider })
