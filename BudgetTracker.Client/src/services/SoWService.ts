import proto, { SoWServiceClient } from '../generated/StateOfTheWorld_grpc_web_pb';
import message from '../generated/StateOfTheWorld_pb';
import { writable } from 'svelte/store';
import { ClientReadableStream } from 'grpc-web';

export class SoWService {
    Empty = new message.Empty()
    SystemInfo = writable(<any>null);

    private reconnect<T>(method: (x: SoWServiceClient) => ClientReadableStream<T>, callback: (x: T) => any) {
        var svc = new proto.SoWServiceClient(document.location.protocol + '//' + document.location.host, null, null);
        method(svc)
        .on("data", response => {
            callback(response);
        })
        .on("end", () => this.reconnect(method, callback))
        .on("error", err => {
            console.error(err);
            setTimeout(() => this.reconnect(method, callback), 1000);
        })
    }

    getSystemInfo() {
        this.reconnect(x=>x.getSystemInfo(this.Empty, undefined), response => {
            let object = response.toObject(false)
            this.SystemInfo.set(object);
        })
    }

    constructor() {
        this.getSystemInfo();
/*
        svc.getState(new message.Empty(), undefined).on("data", (value) => {
            console.log("data:" + value && value.getTimestamp());
            if (value) {
              state = value.getTimestamp();
            }
          }).on("error", err => console.log("err:" + JSON.stringify(err)))
            .on("status", st => console.log("st:" + JSON.stringify(st)))
            .on("end", e => console.log("e:" + e));
  */        
    }
}

const instance = new SoWService();

export default instance;

