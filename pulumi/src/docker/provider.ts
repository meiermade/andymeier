import * as docker from '@pulumi/docker'

export const provider = new docker.Provider('default')
