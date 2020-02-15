import proto, { SoWServicePromiseClient } from '../generated/StateOfTheWorld_grpc_web_pb';
import protoSettings from '../generated/Settings_pb';
import protoCommons from '../generated/Commons_pb';
import { writable } from 'svelte/store';
import { ClientReadableStream } from 'grpc-web';

export class SoWService {
    Empty = new protoCommons.Empty()
    SystemInfo = writable(new protoSettings.SystemInfo().toObject());
    Settings = writable(new protoSettings.Settings().toObject());

    SoWClient: proto.SoWServicePromiseClient;

    private reconnect<T>(method: (x: SoWServicePromiseClient) => ClientReadableStream<T>, callback: (x: T) => any) {
        method(this.SoWClient)
        .on("data", response => {
            callback(response);
        })
        .on("end", () => this.reconnect(method, callback))
        .on("error", err => {
            console.error(err);
            setTimeout(() => this.reconnect(method, callback), 1000);
        })
    }

    async updateSettingsPassword(newPassword: string) {
        const passwordUpdateRequest = new protoSettings.UpdatePasswordRequest();
        passwordUpdateRequest.setNewpassword(newPassword);
        await this.SoWClient.updateSettingsPassword(passwordUpdateRequest);
    }

    async deleteConfig(uuid: protoCommons.UUID.AsObject) {
        var id = new protoCommons.UUID();
        id.setValue(uuid.value);
        await this.SoWClient.deleteConfig(id)
    }

    async clearLastSuccessful(uuid: protoCommons.UUID.AsObject) {
        var id = new protoCommons.UUID();
        id.setValue(uuid.value);
        await this.SoWClient.clearLastSuccesful(id);
    }

    async addConfig(scraperName: string, login: string, password: string) {
        const req = new protoSettings.AddScraperRequest();
        req.setLogin(login);
        req.setPassword(password);
        req.setName(scraperName);
        await this.SoWClient.addScraper(req);
    }

    getSystemInfo() {
        this.reconnect(x=>x.getSystemInfo(this.Empty, undefined), response => {
            let object = response.toObject(false)
            this.SystemInfo.set(object);
        })
    }

    getSettings() {
        this.reconnect(x=>x.getSettings(this.Empty, undefined), response => {
            let object = response.toObject(false);
            this.Settings.set(object);
        })
    }

    constructor() {
        const enableDevTools = (<any>window).__GRPCWEB_DEVTOOLS__ || (() => {});
        this.SoWClient = new proto.SoWServicePromiseClient(document.location.protocol + '//' + document.location.host, null, null);
        enableDevTools([ this.SoWClient ]);

        this.getSystemInfo();
        this.getSettings();
    }
}

const instance = new SoWService();

export default instance;

