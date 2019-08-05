import Auth from './AuthService';

export class RestCache {
  cache: any = {};

  private checkLogin(resp: Response): boolean {
    if (resp.status == 401) {
      Auth.logoff();
      return false;
    }
    return true;
  }

  async get(uri: string, hasResponse: boolean, shouldDeserialize: boolean) : Promise<any> {
    let fetched: Response;

    fetched = await fetch(uri, {method: "GET", headers: { "X-Requested-With": "XMLHttpRequest" }});

    if (!this.checkLogin(fetched)) {
      return null;
    }

    if (hasResponse) {
      if (!shouldDeserialize) {
        return await fetched.text();
      }
      return await fetched.json();
    }
  }

  async post(uri: string, data:any, hasResponse: boolean, shouldDeserialize: boolean) {
    let fetched: Response;

    fetched = await fetch(uri, {method: "POST", headers: { "X-Requested-With": "XMLHttpRequest"}, body: JSON.stringify(data)});

    if (!this.checkLogin(fetched)) {
      return null;
    }

    if (hasResponse) {
      if (!shouldDeserialize) {
        return await fetched.text();
      }
      return await fetched.json();
    }
  }
}

const Instance = new RestCache();

export default Instance;