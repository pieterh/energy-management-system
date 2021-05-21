
// A mock function to mimic making an async request for data
export function login(username :string, secret: string) {
    return new Promise<{ data: any }>((resolve) => {
        console.info("login()");     
        var resp = { ok: true }
        setTimeout(() => resolve({ data: resp }), 500)
    });
  }

  // A mock function to mimic making an async request for data
export function logout() {
    return new Promise<{ data: any }>((resolve) => {
        console.info("logout()");   
        var resp = { ok: true }
        setTimeout(() => resolve({ data: true }), 500)
    });
  }

