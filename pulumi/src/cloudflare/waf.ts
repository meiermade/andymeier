import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import { andymeierZone } from './zone'
import * as config from '../config'

// Use lower(url_decode()) to normalize both case and percent-encoding.
// Scanners use mixed case (e.g. /ReAcT/.EnV) and URL encoding (e.g. %2F)
// to bypass WAF rules. Cloudflare does NOT decode reserved chars like %2F
// during standard normalization, so url_decode() is required.
const p = 'lower(url_decode(http.request.uri.path))'

const expression = [
    // Sensitive dotfiles and directories
    `(${p} contains "/.env")`,
    `(${p} contains "/.git")`,
    `(${p} contains "/.aws")`,
    `(${p} contains "/.ssh")`,
    `(${p} contains "/.terraform")`,

    // CMS and framework probes
    `(${p} contains "/wp-")`,
    `(${p} contains "/wordpress")`,
    `(${p} contains "/xmlrpc")`,
    `(${p} contains "/phpmyadmin")`,
    `(${p} contains "/pma")`,

    // Admin and server management
    `(${p} contains "/admin")`,
    `(${p} contains "/cgi-bin")`,
    `(${p} contains "/actuator")`,
    `(${p} contains "/solr")`,
    `(${p} contains "/telescope")`,
    `(${p} contains "/vendor")`,
    `(${p} contains "/invoker")`,
    `(${p} contains "/balancer-manager")`,
    `(${p} contains "/login")`,

    // Credential and config probes
    `(${p} contains "/credentials")`,
    `(${p} contains "/known_hosts")`,
    `(${p} contains "sendgrid")`,
    `(${p} contains "codecommit")`,
    `(${p} contains "/env.cfg")`,
    `(${p} contains "/api/config")`,
    `(${p} contains "/careers_not_hosted")`,

    // Dangerous file extensions
    `(${p} contains ".php")`,
    `(${p} contains ".asp")`,
    `(${p} contains ".jsp")`,
    `(${p} contains ".cgi")`,
    `(${p} contains ".yml")`,
    `(${p} contains ".xml")`,
    `(${p} contains ".bak")`,
    `(${p} contains ".rb")`,

    // Env file variants (with dots in name like .env.sample, .env.prod)
    `(${p} contains ".env.")`,
].join(' or ')

new cloudflare.Ruleset(`${config.identifier}-waf`, {
    zoneId: andymeierZone.id,
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
