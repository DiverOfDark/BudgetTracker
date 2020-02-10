import proto from '../generated/StateOfTheWorld_grpc_web_pb';
import message from '../generated/StateOfTheWorld_pb';
import { writable } from 'svelte/store';

export class SoWService {
    Empty = new message.Empty()
    SystemInfo = writable(<any>null);

    getSystemInfo() {
        var svc = new proto.SoWServiceClient(document.location.protocol + '//' + document.location.host, null, null);
        svc.getSystemInfo(this.Empty, undefined)
        .on("data", response => {
            let object = response.toObject(false)
            this.SystemInfo.set(object);
        })
        .on("end", () => this.getSystemInfo())
        .on("error", err => {
            console.error(err);
            setTimeout(() => this.getSystemInfo(), 1000);
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

