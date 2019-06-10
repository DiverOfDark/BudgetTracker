export class RestCache {
  cache: any;

  memoize(method: any) : any {
    let func = async function(this: any) {
      let args = JSON.stringify(arguments);

      window.rest.cache = window.rest.cache || {};
      window.rest.cache[args] = window.rest.cache[args] || method.apply(this, arguments);

      return window.rest.cache[args];
    };

    return func;
  }

  cachedQuery(uri: string, method: string, hasResponse: boolean, shouldDeserialize: boolean) : Promise<any> {
    return window.rest.memoize(window.rest.query)(uri, method, hasResponse, shouldDeserialize);
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

declare global {
  interface Window { rest: RestCache; }
}

window.rest = window.rest || new RestCache()