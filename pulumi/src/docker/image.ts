import * as pulumi from '@pulumi/pulumi'
import * as dockerBuild from '@pulumi/docker-build'
import * as path from 'path'
import * as config from '../config'

export const image = new dockerBuild.Image(config.identifier, {
    tags: [
        pulumi.interpolate`${config.dockerConfig.registryUri}/${config.identifier}`
    ],
    context: {
        location: path.join(config.rootDir, 'app')
    },
    platforms: [
        dockerBuild.Platform.Linux_amd64
    ],
    push: true,
})

export const imageRef = image.ref
