import * as pulumi from '@pulumi/pulumi'
import * as random from '@pulumi/random'
import * as path from 'path'

const randomProvider = new random.Provider('default')

export const env = pulumi.getStack()

export const rootDir = path.dirname(path.dirname(__dirname))

const identityStack = new pulumi.StackReference('meiermade/identity/prod')
export const eksNodeManagerArn = identityStack.requireOutput('eksNodeManagerArn')

const infrastructureStack = new pulumi.StackReference('meiermade/infrastructure/prod')
export const andrewmeierProdNamespace = infrastructureStack.requireOutput('andrewmeierProdNamespace')
export const kubeconfig = infrastructureStack.requireOutput('kubeconfig')

const andrewmeierIdentityStack = new pulumi.StackReference('meiermade/andrewmeier-identity/prod')
export const awsRegion = andrewmeierIdentityStack.requireOutput('awsRegion')
export const awsAccountId = andrewmeierIdentityStack.requireOutput('awsAccountId')

export const domain = env === 'prod' ? 'andrewmeier.dev' : 'andrewmeier.net'

export const identifier = `andrewmeier-${env}`

export const awsConfig = {
    accountId: awsAccountId,
    region: awsRegion
}

const rawSeqConfig = new pulumi.Config('seq')
export const seqConfig = {
    endpoint: rawSeqConfig.require('endpoint'),
    apiKey: rawSeqConfig.requireSecret('apiKey')
}

const tunnelRandomPassword = new random.RandomPassword(`${identifier}-tunnel`, {
    length: 32,
    special: false
}, { provider: randomProvider })

const rawCloudflareConfig = new pulumi.Config('cloudflare')
export const cloudflareConfig = {
    accountId: rawCloudflareConfig.require('accountId'),
    apiToken: rawCloudflareConfig.require('apiToken'),
    tunnelSecret: pulumi.secret(tunnelRandomPassword.result),
    cloudflaredVersion: '2024.9.1'
}
