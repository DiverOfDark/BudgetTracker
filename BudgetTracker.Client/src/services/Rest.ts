export class RestCache {
  cache: any = {};

  async get(uri: string, hasResponse: boolean, shouldDeserialize: boolean) : Promise<any> {
    let fetched: Response;

    fetched = await fetch(uri, {method: "GET", headers: { "X-Requested-With": "XMLHttpRequest" }});

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