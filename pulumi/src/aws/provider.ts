import * as aws from '@pulumi/aws'
import * as config from '../config'

export const provider = new aws.Provider('default', {
    region: config.awsConfig.region,
    allowedAccountIds: [ config.awsConfig.accountId ]
})
