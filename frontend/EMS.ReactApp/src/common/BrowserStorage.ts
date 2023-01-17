
class localC {
    get(key: string){
        var value = localStorage.getItem(key);
        return JSON.parse(value || "{}");
    }

    set(key:string, value: any) {
        localStorage.setItem(key, JSON.stringify(value));        
    }

    removeItem(key: string){
        localStorage.removeItem(key);                
    }

    has(key: string){
        var value = localStorage.getItem(key);
        return value !== null;        
    }
}

class sessionC {
    get(key: string){
        var value = sessionStorage.getItem(key);
        return JSON.parse(value || "{}");
    } 

    set(key:string, value: any) {
        sessionStorage.setItem(key, JSON.stringify(value));        
    }

    removeItem(key: string){
        sessionStorage.removeItem(key);        
    }

    has (key: string){
        var value = sessionStorage.getItem(key);
        return value !== null;        
    }
}

export class browserStorage {
    static session: sessionC = new sessionC();
    static local: localC = new localC(); 
}