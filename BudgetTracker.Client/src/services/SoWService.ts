import proto, { SoWServicePromiseClient } from '../generated/StateOfTheWorld_grpc_web_pb';
import protoSettings from '../generated/Settings_pb';
import protoDebts from '../generated/Debts_pb';
import protoCommons from '../generated/Commons_pb';
import protoSpentCategories from '../generated/SpentCategories_pb';
import protoPayments from '../generated/Payments_pb';
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

    async deleteDebt(id: protoCommons.UUID.AsObject) {
        let request = new protoCommons.UUID();
        request.setValue(id.value);
        await this.SoWClient.deleteDebt(request);
    }

    async updateDebt(debt: protoDebts.Debt.AsObject) {
        let request = new protoDebts.Debt();
        let uuid = new protoCommons.UUID();

        if (debt.id) {
            uuid.setValue(debt.id.value);
        }
        request.setId(uuid);
        request.setAmount(debt.amount);
        request.setCcy(debt.ccy);
        request.setDaysCount(debt.daysCount);
        request.setDescription(debt.description);
        let issuedModel = new protoCommons.Timestamp();
        issuedModel.setSeconds(debt.issued!.seconds);
        issuedModel.setNanos(debt.issued!.nanos);
        request.setIssued(issuedModel);
        request.setPercentage(debt.percentage);
        request.setRegexForTransfer(debt.regexForTransfer);
        await this.SoWClient.editDebt(request);
    }

    async deleteSpentCategory(id: protoCommons.UUID.AsObject) {
        let request = new protoCommons.UUID();
        request.setValue(id.value);
        await this.SoWClient.deleteSpentCategory(request);
    }

    async updateSpentCategory(category: protoSpentCategories.SpentCategory.AsObject) {
        let request = new protoSpentCategories.SpentCategory();
        let uuid = new protoCommons.UUID();

        if (category.id) {
            uuid.setValue(category.id.value);
        }
        request.setId(uuid);
        request.setCategory(category.category);
        request.setPattern(category.pattern);
        request.setKind(category.kind);

        await this.SoWClient.editSpentCategory(request);
    }

    async createSpentCategory(category: string, pattern: string, kind: protoPayments.PaymentKind) {
        let categoryObject = new protoSpentCategories.SpentCategory().toObject();
        categoryObject.category = category;
        categoryObject.pattern = pattern;
        categoryObject.kind = kind;
        this.updateSpentCategory(categoryObject);
    }

    async showCategorized(showCategorized: boolean) {
        let request = new protoPayments.ShowCategorizedRequest();
        request.setShowCategorized(showCategorized);
        await this.SoWClient.showCategorized(request);
    }

    async setOrdering(newOrdering: number) {
        let request = new protoPayments.UpdateOrderingRequest();
        request.setNewOrdering(newOrdering);
        await this.SoWClient.setOrdering(request);
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

    getDebts(callback: (debts: protoDebts.DebtsStream.AsObject) => void): () => void {
        return this.reconnect(x=>x.getDebts(this.Empty), response => {
            let object = response.toObject(false);
            callback(object);
        })
    }

    getSpentCategories(callback: (spentCategories: protoSpentCategories.SpentCategoriesStream.AsObject) => void) : () => void {
        return this.reconnect(x=>x.getSpentCategories(this.Empty), response => {
            let object = response.toObject(false);
            callback(object);
        })
    }

    getPayments(callback: (payments: protoPayments.PaymentsStream.AsObject) => void) : () => void {
        return this.reconnect(x=>x.getPayments(this.Empty), response => {
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

