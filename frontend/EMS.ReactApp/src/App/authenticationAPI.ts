// A mock function to mimic making an async request for data
export function login(username :string, secret: string) {
    return new Promise<{ data: boolean }>((resolve) => {
        console.info("login()");     
        setTimeout(() => resolve({ data: true }), 2500)
    });
  }

  // A mock function to mimic making an async request for data
export function logout() {
    return new Promise<{ data: boolean }>((resolve) => {
        console.info("logout()");   
        setTimeout(() => resolve({ data: true }), 2500)
    });
  }
