import axios, { AxiosRequestConfig, AxiosHeaders } from 'axios';
import URLParse from 'url-parse';

import { browserStorage } from '../../common/BrowserStorage'
import { store }  from '../../app/store';
import { relogin } from './authenticationSlice';

// make sure that this module is
// included _after_ the initializion of the store and 
// included _before_ any axios methods are called
export function AddInterceptors() {
    // interceptor that will check for authentication errors
    // in the case there is an authentication error, it will
    // perform an action so that the user is able to perform
    // a new authentication
    axios.interceptors.response.use(function (response) {
        return response;
    }, function (error) {
        if (error.response.status === 401) {
            try{    
                // we only need to perform a relogin when the authentication
                // error occured when we are logged in (ie. when the token did expire)  
                if (store.getState().authentication.isLoggedIn){
                    store.dispatch(relogin());
                }
            }catch(error){
                console.log(error);
            }
        } else {
            return Promise.reject(error);
        }
    });

    // interceptor that will add the bearer token in the header when 
    // we have a token in the browser sessions storage
    // and are accessing the api
    axios.interceptors.request.use(function (config: AxiosRequestConfig) {
        var parsedUrl = URLParse(config.url as string, true);
        if (parsedUrl.pathname.startsWith('/api') && config.headers != null){
            const token = browserStorage.session.get('token');
            if (token !== undefined && token !== null) {
                (config.headers as AxiosHeaders).set("Authorization", `Bearer ${token}`);
            }                
        }
        return config; 
    });
}
