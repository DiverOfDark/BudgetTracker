import promiseMemoize from 'promise-memoize';

window.rest = window.rest || class RestCache {
  static memoize(method) {
    return async function() {
      let args = JSON.stringify(arguments);

      window.rest.cache = window.rest.cache || {};
      window.rest.cache[args] = window.rest.cache[args] || method.apply(this, arguments);

      return window.rest.cache[args];
    };
  }

  static cachedQuery(uri) {
    return window.rest.memoize(window.rest.query)(uri);
  }

  static async query(uri) {
    let fetched = await fetch(uri);
    return await fetched.json();
  }
};

export function systemInfo() { 
  return window.rest.cachedQuery(`/System/SiteInfo`);
}

export function table(provider, from) {
  return window.rest.query(`/Table/IndexJson?provider=` + provider + `&startingFrom=` + from);
}