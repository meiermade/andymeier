import * as dockerBuild from '@pulumi/docker-build'

export const provider = new dockerBuild.Provider('default')
