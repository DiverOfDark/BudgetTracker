export class RestCache {
  cache: any = {};

  cachedQuery(uri: string, method: string, hasResponse: boolean, shouldDeserialize: boolean) : Promise<any> {
    let self = this;
    let func = async function(uri: string, method: string, hasResponse: boolean, shouldDeserialize: boolean) {
      let args = JSON.stringify(arguments);

      self.cache[args] = self.cache[args] || self.query(uri, method, hasResponse, shouldDeserialize);

      return self.cache[args];
    };

    return func(uri, method, hasResponse, shouldDeserialize);
  }

  async query(uri: string, method: string, hasResponse: boolean, shouldDeserialize: boolean) : Promise<any> {
    let fetched: Response;
    if (!method) {
      method = "GET";
    }

    fetched = await fetch(uri, {method: method, headers: { "X-Requested-With": "XMLHttpRequest" }});

    if (hasResponse) {
      if (!shouldDeserialize) {
        return await fetched.text();
      }
      return await fetched.json();
    }
  }
};

const Instance = new RestCache();

export default Instance;