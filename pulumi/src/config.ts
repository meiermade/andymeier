import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'

export const rootDir = path.dirname(path.dirname(__dirname))

export const identifier = 'andymeier'

export const rawDockerConfig = new pulumi.Config('docker')

export const dockerConfig = {
    registryUri: rawDockerConfig.require('registryUri'),
}

const rawCloudflareConfig = new pulumi.Config('cloudflare')

export const cloudflareConfig = {
    accountId: rawCloudflareConfig.require('accountId'),
    apiToken: rawCloudflareConfig.requireSecret('apiToken'),
    cloudflaredVersion: '2026.2.0'
}

const rawK8sConfig = new pulumi.Config('k8s')

export const k8sConfig = {
    namespace: rawK8sConfig.require('namespace')
}

const rawSeqConfig = new pulumi.Config('seq')

export const seqConfig = {
    endpoint: rawSeqConfig.require('endpoint')
}

const rawNotionConfig = new pulumi.Config('notion')

export const notionConfig = {
    articlesDatabaseId: rawNotionConfig.require('articlesDatabaseId'),
    apiKey: rawNotionConfig.requireSecret('apiKey')
}
