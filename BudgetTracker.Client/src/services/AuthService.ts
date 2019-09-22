import { writable } from 'svelte/store';

export class AuthService {
    authorized = writable(true);

    public getStore() {
        return this.authorized;
    }

    public logoff() {
        this.authorized.set(false);
    }

    public logon() {
        this.authorized.set(true);
    }
}

const instance = new AuthService();

export default instance;