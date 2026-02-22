import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as path from 'path'
import { provider } from './provider'
import * as repository from '../aws/repository'
import * as config from '../config'

const registry:pulumi.Output<docker.types.input.Registry> =
    repository.credentials.apply(creds => {
        let decoded = Buffer.from(creds.authorizationToken, 'base64').toString('utf-8')
        let [username, password] = decoded.split(':')
        return {
            server: creds.proxyEndpoint,
            username: username,
            password: password
        }
    })

export const image = new docker.Image(config.identifier, {
    imageName: repository.repo.repositoryUrl,
    build: {
        context: path.join(config.rootDir, 'app'),
        platform: 'linux/arm64'
    },
    registry: registry
}, { provider })

export const imageName = image.repoDigest.apply(digest => digest!)
