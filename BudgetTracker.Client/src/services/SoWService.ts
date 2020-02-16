import proto, { SoWServicePromiseClient } from '../generated/StateOfTheWorld_grpc_web_pb';
import protoSettings from '../generated/Settings_pb';
import protoCommons from '../generated/Commons_pb';
import { writable } from 'svelte/store';
import { ClientReadableStream } from 'grpc-web';

export class SoWService {
    Empty = new protoCommons.Empty()
    SystemInfo = writable(new protoSettings.SystemInfo().toObject());
    SoWClient: proto.SoWServicePromiseClient;

    private reconnect<T>(method: (x: SoWServicePromiseClient) => ClientReadableStream<T>, callback: (x: T) => any): () => void {
        let that = this;

        function reconnectImpl() {
            console.log("Stream reconnecting... (" + method.toString() + ")");

            let cancelResult = () => {};

            let result = method(that.SoWClient)
            .on("data", response => {
                callback(response);
            })
            .on("end", () => {
                cancelResult = reconnectImpl();
            })
            .on("error", err => {
                console.error(err);
                setTimeout(() => {
                    cancelResult = reconnectImpl();
                }, 1000);
            });

            cancelResult = () => result.cancel();

            return () => {
                console.log("Stream canceled. (" + method.toString() + ")");
                cancelResult();
            }
        }

        let cancel = reconnectImpl();

        // Handle page visibility change events
        function handleVisibilityChange() {
            if (document.visibilityState == "hidden") {
                cancel();
            } else {
                cancel = reconnectImpl();
            }
        }
          
        document.addEventListener('visibilitychange', handleVisibilityChange, false);

        return () => {
            cancel();
            console.log("Stream aborted. (" + method.toString() + ")");
            document.removeEventListener("visibilitychange", handleVisibilityChange, false);
        };
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

    async downloadDump(): Promise<Uint8Array> {
        const dump = await this.SoWClient.downloadDbDump(this.Empty);
        return dump.getContent_asU8();
    }

    async executeScript(command: string): Promise<protoSettings.ExecuteScriptResponse.AsObject> {
        let request = new protoSettings.ExecuteScriptRequest();
        request.setScript(command);
        let response = await this.SoWClient.executeScript(request);
        return response.toObject();
    }

    getScreenshot(callback: (base64Image: string) => void): () => void  {
        return this.reconnect(x => x.getScreenshot(this.Empty), response => {
            let object = response.getContents_asB64();
            callback(object);
        })
    }

    getSystemInfo(): () => void  {
        return this.reconnect(x=>x.getSystemInfo(this.Empty), response => {
            let object = response.toObject(false)
            this.SystemInfo.set(object);
        })
    }

    getSettings(callback: (settings: protoSettings.Settings.AsObject) => void): () => void {
        return this.reconnect(x=>x.getSettings(this.Empty), response => {
            let object = response.toObject(false);
            callback(object);
        })
    }

    constructor() {
        const enableDevTools = (<any>window).__GRPCWEB_DEVTOOLS__ || (() => {});
        this.SoWClient = new proto.SoWServicePromiseClient(document.location.protocol + '//' + document.location.host, null, null);
        enableDevTools([ this.SoWClient ]);

        this.getSystemInfo();
    }
}

const instance = new SoWService();

export default instance;

