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

    
    function jsonToFormData (inJSON: any, inTestJSON: any = null, inFormData: FormData = new FormData(), parentKey: string = ''): FormData {
      // http://stackoverflow.com/a/22783314/260665
      // Raj: Converts any nested JSON to formData.
      var form_data = inFormData || new FormData();
      var testJSON = inTestJSON || {};
      for ( var key in inJSON ) {
          // 1. If it is a recursion, then key has to be constructed like "parent.child" where parent JSON contains a child JSON
          // 2. Perform append data only if the value for key is not a JSON, recurse otherwise!
          var constructedKey = key;
          if (parentKey) {
              constructedKey = parentKey + "." + key;
          }

          var value = inJSON[key];
          if (value && value.constructor === {}.constructor) {
              // This is a JSON, we now need to recurse!
              jsonToFormData (value, testJSON, form_data, constructedKey);
          } else {
              form_data.append(constructedKey, inJSON[key]);
              testJSON[constructedKey] = inJSON[key];
          }
      }
      return form_data;
    }

    fetched = await fetch(uri, {method: "POST", headers: { "X-Requested-With": "XMLHttpRequest" }, body: jsonToFormData(data)});

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