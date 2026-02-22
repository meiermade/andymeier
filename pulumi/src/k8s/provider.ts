import * as k8s from '@pulumi/kubernetes'
import * as config from '../config'

export const provider = new k8s.Provider('default', {
    kubeconfig: config.kubeconfig
})
