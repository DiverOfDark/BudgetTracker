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

  cachedQuery(uri: string) : Promise<any> {
    return window.rest.memoize(window.rest.query)(uri);
  }

  async query(uri: string) : Promise<any> {
    let fetched = await fetch(uri);
    return await fetched.json();
  }
};

declare global {
  interface Window { rest: RestCache; }
}

window.rest = window.rest || new RestCache()