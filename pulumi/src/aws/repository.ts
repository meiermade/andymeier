import * as aws from '@pulumi/aws'
import { provider } from './provider'
import * as config from '../config'

export const repo = new aws.ecr.Repository(config.identifier, {
    name: config.identifier
}, { provider })

export const credentials = aws.ecr.getCredentialsOutput({
    registryId: repo.registryId
}, { provider })

new aws.ecr.RepositoryPolicy(config.identifier, {
    repository: repo.name,
    policy: {
        Version: '2012-10-17',
        Statement: [
            {
                Effect: 'Allow',
                Principal: {
                    AWS: config.eksNodeManagerArn
                },
                Action: [
                    'ecr:GetDownloadUrlForLayer',
                    'ecr:BatchGetImage',
                    'ecr:BatchCheckLayerAvailability'
                ]
            }
        ]
    }
}, { provider: provider })

new aws.ecr.LifecyclePolicy(config.identifier, {
    repository: repo.name,
    policy: {
        rules: [{
            rulePriority: 1,
            selection: {
                tagStatus: 'any',
                countType: 'imageCountMoreThan',
                countNumber: 1
            },
            action: {
                type: 'expire'
            },
        }]
    }
}, { provider })
