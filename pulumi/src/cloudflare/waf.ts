import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import { andymeierZone } from './zone'
import * as config from '../config'

const expression = [
    // Sensitive dotfiles and directories
    '(http.request.uri.path contains "/.env")',
    '(http.request.uri.path contains "/.git")',
    '(http.request.uri.path contains "/.aws")',
    '(http.request.uri.path contains "/.ssh")',
    '(http.request.uri.path contains "/.terraform")',

    // CMS and framework probes
    '(http.request.uri.path contains "/wp-")',
    '(http.request.uri.path contains "/wordpress")',
    '(http.request.uri.path contains "/xmlrpc")',
    '(http.request.uri.path contains "/phpMyAdmin")',
    '(http.request.uri.path contains "/phpmyadmin")',
    '(http.request.uri.path contains "/pma")',

    // Admin and server management
    '(http.request.uri.path contains "/admin")',
    '(http.request.uri.path contains "/cgi-bin")',
    '(http.request.uri.path contains "/actuator")',
    '(http.request.uri.path contains "/solr")',
    '(http.request.uri.path contains "/telescope")',
    '(http.request.uri.path contains "/vendor")',
    '(http.request.uri.path contains "/invoker")',
    '(http.request.uri.path contains "/balancer-manager")',

    // Credential and config probes
    '(http.request.uri.path contains "/credentials")',
    '(http.request.uri.path contains "/known_hosts")',
    '(http.request.uri.path contains "sendgrid")',
    '(http.request.uri.path contains "codecommit")',
    '(http.request.uri.path contains "/env.cfg")',

    // Dangerous file extensions
    '(http.request.uri.path contains ".php")',
    '(http.request.uri.path contains ".asp")',
    '(http.request.uri.path contains ".jsp")',
    '(http.request.uri.path contains ".cgi")',
    '(http.request.uri.path contains ".yml")',
    '(http.request.uri.path contains ".xml")',
    '(http.request.uri.path contains ".bak")',
    '(http.request.uri.path contains ".rb")',
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
