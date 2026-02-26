import * as k8s from '@pulumi/kubernetes'
import * as pulumi from '@pulumi/pulumi'
import { provider } from './provider'
import * as config from '../config'

const identifier = `${config.identifier}-redis`

const statefulSet = new k8s.apps.v1.StatefulSet(identifier, {
    metadata: {
        name: identifier,
        namespace: config.k8sConfig.namespace
    },
    spec: {
        replicas: 1,
        serviceName: identifier,
        selector: {
            matchLabels: {
                app: identifier
            }
        },
        template: {
            metadata: {
                labels: {
                    app: identifier
                }
            },
            spec: {
                containers: [
                    {
                        name: identifier,
                        image: 'redis:8.4.0',
                        ports: [
                            {
                                containerPort: 6379
                            }
                        ],
                        command: ['redis-server'],
                        args: [
                            '--appendonly',
                            'yes'
                        ],
                        readinessProbe: {
                            exec: {
                                command: ['redis-cli', 'ping']
                            },
                            initialDelaySeconds: 5,
                            periodSeconds: 10,
                            timeoutSeconds: 5,
                            failureThreshold: 3
                        },
                        livenessProbe: {
                            exec: {
                                command: ['redis-cli', 'ping']
                            },
                            initialDelaySeconds: 30,
                            periodSeconds: 10,
                            timeoutSeconds: 5,
                            failureThreshold: 3
                        },
                        volumeMounts: [
                            {
                                name: 'data',
                                mountPath: '/data'
                            }
                        ],
                        resources: {
                            requests: {
                                cpu: '250m',
                                memory: '256Mi',
                                'ephemeral-storage': '50Mi'
                            },
                            limits: {
                                cpu: '500m',
                                memory: '512Mi',
                                'ephemeral-storage': '1024Mi'
                            }
                        }
                    }
                ]
            }
        },
        volumeClaimTemplates: [
            {
                metadata: {
                    name: 'data',
                    namespace: config.k8sConfig.namespace
                },
                spec: {
                    accessModes: ['ReadWriteOnce'],
                    resources: {
                        requests: {
                            storage: '2Gi'
                        }
                    }
                }
            }
        ]
    }
}, { provider })

const service = new k8s.core.v1.Service(`${identifier}-service`, {
    metadata: {
        name: identifier,
        namespace: config.k8sConfig.namespace
    },
    spec: {
        type: 'ClusterIP',
        selector: {
            app: identifier
        },
        ports: [
            {
                port: 6379,
                targetPort: 6379
            }
        ]
    }
}, { provider, dependsOn: statefulSet })

export const host = pulumi.interpolate`${service.metadata.name}.${service.metadata.namespace}`
export const port = 6379
