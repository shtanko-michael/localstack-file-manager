export default class AppSettings {
    public api: string;
    public cloudFront: string;

    constructor(settings: any) {
        this.api = settings.api;
        this.cloudFront = settings.cloudFront;
    }
}