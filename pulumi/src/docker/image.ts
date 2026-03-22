import * as pulumi from '@pulumi/pulumi'
import * as dockerBuild from '@pulumi/docker-build'
import * as path from 'path'
import * as config from '../config'
import { provider } from './provider'

const registryUri = config.dockerConfig.registryUri
const registryHost = registryUri.split('/')[0]

export const image = new dockerBuild.Image(config.identifier, {
    tags: [
        pulumi.interpolate`${registryUri}/${config.identifier}`
    ],
    context: {
        location: path.join(config.rootDir, 'app')
    },
    platforms: [
        dockerBuild.Platform.Linux_amd64
    ],
    push: true,
    registries: [{
        address: registryHost,
        username: 'oauth2accesstoken',
        password: config.dockerConfig.registryAccessToken,
    }],
}, { provider })

export const imageRef = image.ref
