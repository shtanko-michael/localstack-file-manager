export enum Env {
    dev = 'Development',
    prod = 'Production'
}

// @ts-ignore
const env: Env = process.env.NODE_ENV == 'development' ? Env.dev : Env.prod;
export {
    env
}