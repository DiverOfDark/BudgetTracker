import proto, { SoWServicePromiseClient } from '../generated/StateOfTheWorld_grpc_web_pb';
import protoSettings from '../generated/Settings_pb';
import protoDebts from '../generated/Debts_pb';
import protoCommons from '../generated/Commons_pb';
import protoSpentCategories from '../generated/SpentCategories_pb';
import protoPayments from '../generated/Payments_pb';
import protoAccounts from '../generated/Accounts_pb';
import { writable, get } from 'svelte/store';
import { ClientReadableStream } from 'grpc-web';
import {compare } from '../services/Shared';

class PaymentsStreamViewModel {
    payments = writable<protoPayments.MonthSummary.AsObject[]>([]);
    showCategorized = writable<boolean>(false);

    private parsePayments(stream: protoPayments.PaymentsStream.AsObject) {
        if (stream.snapshot) {
            this.payments.set(stream.snapshot.paymentsList);
            this.showCategorized.set(stream.snapshot.showCategorized);
        } else if (stream.added) {
            let current: protoPayments.MonthSummary.AsObject[] = get(this.payments);
            current.splice(stream.added.position, 0, stream.added.view!!)
            this.payments.set(current);
        } else if (stream.removed) {
            let current: protoPayments.MonthSummary.AsObject[] = get(this.payments);
            current.splice(stream.removed.position, 1)
            this.payments.set(current);
        } else if (stream.updated) {
            var current: protoPayments.MonthSummary.AsObject[] = get(this.payments);
            current.splice(stream.updated.position, 1, stream.updated.view!!)
            this.payments.set(current);
        }
    }

    constructor(service: SoWService, onDestroy: (callback:() => void) => void) {
        let callback = service.reconnect(x=>x.getPayments(service.Empty), response => {
            let object = response.toObject(false);
            this.parsePayments(object);
        })
        onDestroy(callback)
    }
}

class SpentCategoriesStreamViewModel {
    spentCategories = writable<protoSpentCategories.SpentCategory.AsObject[]>([])

    private categorySort(a: protoSpentCategories.SpentCategory.AsObject, b: protoSpentCategories.SpentCategory.AsObject): number {
        return compare(a.category, b.category) * 10 + compare(a.pattern, b.pattern);
    }

    private parseSpentCategories(stream: protoSpentCategories.SpentCategoriesStream.AsObject) {
        if (stream.added) {
            let newCategories = get(this.spentCategories);
            newCategories = [...newCategories, stream.added];
            newCategories.sort(this.categorySort);
            this.spentCategories.set(newCategories);
        } else if (stream.removed) {
            let newCategories = get(this.spentCategories);
            newCategories = newCategories.filter((f: protoSpentCategories.SpentCategory.AsObject) => f.id!.value != stream.removed!.id!.value);
            newCategories.sort(this.categorySort);
            this.spentCategories.set(newCategories);
        } else if (stream.updated) {
            let newCategories = get(this.spentCategories);
            newCategories = newCategories.map((f: protoSpentCategories.SpentCategory.AsObject) => {
                if (f.id!.value == stream.updated!.id!.value) {
                    return stream!.updated;
                }
                return f;
            });
            newCategories.sort(this.categorySort);
            this.spentCategories.set(newCategories);
        } else if (stream.snapshot) {
            let newCategories = stream.snapshot.spentCategoriesList;
            newCategories.sort(this.categorySort);
            this.spentCategories.set(newCategories);
        } else {
            console.log(stream);
            console.error("Unsupported operation");
        }
    }

    constructor(service: SoWService, onDestroy: (callback:() => void) => void) {
        let callback = service.reconnect(x=>x.getSpentCategories(service.Empty), response => {
            let object = response.toObject(false);
            this.parseSpentCategories(object);
        })
        onDestroy(callback)
    }    
}

class DebtsStreamViewModel {
    debts = writable<protoDebts.DebtView.AsObject[]>([]);

    private parseDebts(stream: protoDebts.DebtsStream.AsObject) {
        if (stream.added) {
            let newDebts = get(this.debts);
            newDebts = [...newDebts, stream.added];
            this.debts.set(newDebts);
        } else if (stream.removed) {
            let newDebts = get(this.debts);
            newDebts = newDebts.filter((f: protoDebts.DebtView.AsObject) => f.model!.id!.value != stream.removed!.model!.id!.value);
            this.debts.set(newDebts);
        } else if (stream.updated) {
            let newDebts = get(this.debts);
            newDebts = newDebts.map((f: protoDebts.DebtView.AsObject) => {
                if (f.model!.id!.value == stream.updated!.model!.id!.value) {
                    return stream!.updated;
                }
                return f;
            });
            this.debts.set(newDebts);
        } else if (stream.snapshot) {
            let newStores = stream.snapshot.debtsList;
            this.debts.set(newStores);
        } else {
            console.log(stream);
            console.error("Unsupported operation");
        }
    }

    constructor(service: SoWService, onDestroy: (callback:() => void) => void) {
        let callback = service.reconnect(x=>x.getDebts(service.Empty), response => {
            let object = response.toObject(false);
            this.parseDebts(object);
        });
        onDestroy(callback)
    }    
}

class MoneyColumnMetadataStreamViewModel {
    moneyColumnMetadatas = writable<protoAccounts.MoneyColumnMetadata.AsObject[]>([]);

    private parseMoneyColumnMetadatas(stream: protoAccounts.MoneyColumnMetadataStream.AsObject) {
        if (stream.added) {
            let newMetadatas = get(this.moneyColumnMetadatas);
            newMetadatas = [...newMetadatas, stream.added];
            this.moneyColumnMetadatas.set(newMetadatas);
        } else if (stream.removed) {
            let newMetadatas = get(this.moneyColumnMetadatas);
            newMetadatas = newMetadatas.filter((f: protoAccounts.MoneyColumnMetadata.AsObject) => f.id!.value != stream.removed!.id!.value);
            this.moneyColumnMetadatas.set(newMetadatas);
        } else if (stream.updated) {
            let newMetadatas = get(this.moneyColumnMetadatas);
            newMetadatas = newMetadatas.map((f: protoAccounts.MoneyColumnMetadata.AsObject) => {
                if (f.id!.value == stream.updated!.id!.value) {
                    return stream!.updated;
                }
                return f;
            });
            this.moneyColumnMetadatas.set(newMetadatas);
        } else if (stream.snapshot) {
            let newStores = stream.snapshot.moneyColumnMetadatasList;
            this.moneyColumnMetadatas.set(newStores);
        } else {
            console.log(stream);
            console.error("Unsupported operation");
        }
    }

    constructor(service: SoWService, onDestroy: (callback:() => void) => void) {
        let callback = service.reconnect(x=>x.getMoneyColumnMetadata(service.Empty), response => {
            let object = response.toObject(false);
            this.parseMoneyColumnMetadatas(object);
        });
        onDestroy(callback)
    }   
}

export class SoWService {
    Empty = new protoCommons.Empty()
    SystemInfo = writable(new protoSettings.SystemInfo().toObject());
    SoWClient: proto.SoWServicePromiseClient;

    reconnect<T>(method: (x: SoWServicePromiseClient) => ClientReadableStream<T>, callback: (x: T) => any): () => void {
        let that = this;

        function reconnectImpl() {
            console.log(new Date().toUTCString() + " - Stream reconnecting... (" + method.toString() + ")");

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
                console.log(new Date().toUTCString() + " - Stream canceled. (" + method.toString() + ")");
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
            console.log(new Date().toUTCString() + " - Stream aborted. (" + method.toString() + ")");
            document.removeEventListener("visibilitychange", handleVisibilityChange, false);
        };
    }

    private toUuid(uuid: protoCommons.UUID.AsObject | undefined): protoCommons.UUID {
        let result = new protoCommons.UUID();
        if (uuid !== undefined) {
            result.setValue(uuid.value);
        }
        return result;
    }

    private toTimestamp(timestamp: protoCommons.Timestamp.AsObject | undefined): protoCommons.Timestamp {
        let result = new protoCommons.Timestamp();
        if (timestamp !== undefined) {
            result.setSeconds(timestamp.seconds);
            result.setNanos(timestamp.nanos);
        }
        return result;
    }

    async updateSettingsPassword(newPassword: string) {
        const passwordUpdateRequest = new protoSettings.UpdatePasswordRequest();
        passwordUpdateRequest.setNewpassword(newPassword);
        await this.SoWClient.updateSettingsPassword(passwordUpdateRequest);
    }

    async deleteConfig(uuid: protoCommons.UUID.AsObject) {
        await this.SoWClient.deleteConfig(this.toUuid(uuid))
    }

    async clearLastSuccessful(uuid: protoCommons.UUID.AsObject) {
        await this.SoWClient.clearLastSuccesful(this.toUuid(uuid));
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
        await this.SoWClient.deleteDebt(this.toUuid(id));
    }

    async updateDebt(debt: protoDebts.Debt.AsObject) {
        let request = new protoDebts.Debt();
        request.setId(this.toUuid(debt.id));
        request.setAmount(debt.amount);
        request.setCcy(debt.ccy);
        request.setDaysCount(debt.daysCount);
        request.setDescription(debt.description);
        request.setIssued(this.toTimestamp(debt.issued));
        request.setPercentage(debt.percentage);
        request.setRegexForTransfer(debt.regexForTransfer);
        await this.SoWClient.editDebt(request);
    }

    async deleteSpentCategory(id: protoCommons.UUID.AsObject) {
        await this.SoWClient.deleteSpentCategory(this.toUuid(id));
    }

    async updateSpentCategory(category: protoSpentCategories.SpentCategory.AsObject) {
        let request = new protoSpentCategories.SpentCategory();
        request.setId(this.toUuid(category.id));
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

    async expandCollapse(uuids: protoCommons.UUID.AsObject[]) {
        let request = new protoPayments.ExpandCollapse();
        request.setPathList(uuids.map(this.toUuid))
        await this.SoWClient.expandCollapse(request);
    }

    async deletePayment(id: protoCommons.UUID.AsObject) {
        await this.SoWClient.deletePayment(this.toUuid(id));
    }

    async editPayment(payment: protoPayments.Payment.AsObject) {
        let request = new protoPayments.Payment();
        request.setAmount(payment.amount);
        request.setCategoryId(this.toUuid(payment.categoryId));
        request.setCcy(payment.ccy);
        request.setColumnId(this.toUuid(payment.columnId));
        request.setDebtId(this.toUuid(payment.debtId));
        request.setId(this.toUuid(payment.id));
        request.setKind(payment.kind);
        request.setWhat(payment.what);
        request.setWhen(this.toTimestamp(payment.when));
        await this.SoWClient.editPayment(request);
    }

    async splitPayment(payment: protoPayments.Payment.AsObject, amount: number) {
        let request = new protoPayments.SplitPaymentRequest();
        request.setId(this.toUuid(payment.id));
        request.setAmount(amount);
        await this.SoWClient.splitPayment(request);
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

    getDebts(onDestroy: (callback: () => void) => void): DebtsStreamViewModel {
        return new DebtsStreamViewModel(this, onDestroy);
    }

    getSpentCategories(onDestroy: (callback: () => void) => void): SpentCategoriesStreamViewModel {
        return new SpentCategoriesStreamViewModel(this, onDestroy);
    }

    getPayments(onDestroy: (callback: () => void) => void) : PaymentsStreamViewModel {
        return new PaymentsStreamViewModel(this, onDestroy);
    }

    getMoneyColumnMetadatas(onDestroy: (callback: () => void) => void) : MoneyColumnMetadataStreamViewModel {
        return new MoneyColumnMetadataStreamViewModel(this, onDestroy);
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

