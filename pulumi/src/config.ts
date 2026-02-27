import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'

export const rootDir = path.dirname(path.dirname(__dirname))

export const identifier = 'andrewmeier'

const rawAwsConfig = new pulumi.Config('aws')
const rawCloudflareConfig = new pulumi.Config('cloudflare')
const rawK8sConfig = new pulumi.Config('k8s')
const rawSeqConfig = new pulumi.Config('seq')
const rawNotionConfig = new pulumi.Config('notion')

export const awsConfig = {
    platformAccountId: rawAwsConfig.require('platformAccountId'),
    region: rawAwsConfig.require('region'),
    eksNodeManagerArn: rawAwsConfig.require('eksNodeManagerArn')
}

export const cloudflareConfig = {
    accountId: rawCloudflareConfig.require('accountId'),
    apiToken: rawCloudflareConfig.requireSecret('apiToken'),
    zoneName: rawCloudflareConfig.require('zoneName'),
    cloudflaredVersion: '2026.2.0'
}

export const k8sConfig = {
    namespace: rawK8sConfig.require('namespace')
}

export const seqConfig = {
    endpoint: rawSeqConfig.require('endpoint'),
    apiKey: rawSeqConfig.requireSecret('apiKey')
}

export const notionConfig = {
    articlesDatabaseId: rawNotionConfig.require('articlesDatabaseId'),
    token: rawNotionConfig.requireSecret('token')
}
