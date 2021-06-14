import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import  browserStorage  from 'store2';
import axios from 'axios';
import { RootState } from '../../common/hooks';
import { login, logout } from './authenticationAPI';

enum LoginStateEnum {
  'logged_out', 'log_in' , 'logged_in' , 'log_out'
}

export interface LoginState {
    state: LoginStateEnum;
    isLoggedIn: boolean;
    token: string | undefined;
    user: User | undefined;
    hasAuthenticationError: boolean;
    message: string | undefined;
  }
  
function  CreateState() : LoginState {
  var newState : LoginState=  { 
    state: LoginStateEnum.logged_out, 
    isLoggedIn: false,
    token: undefined, 
    user: undefined,
    hasAuthenticationError: false,
    message: undefined
  };

  // TODO: check expiration time
  var token = browserStorage.session.get('token');
  if (token !== undefined && token !== null){
    newState.state = LoginStateEnum.logged_in;
  }

  // retrieve authentication error and message from local session storage
  var hasAuthenticationError = browserStorage.session.get('hasAuthenticationError');
  if (hasAuthenticationError === true) {
    newState.hasAuthenticationError = true;
  }

  var message = browserStorage.session.get('message');
  if (message !== undefined && message !== null) {
    newState.message = message;
  }

  browserStorage.session.remove("hasAuthenticationError");
  browserStorage.session.remove("message");
  return newState;  
}

const initialState = CreateState();

function UpdateState(state: LoginState, s : LoginStateEnum, user?:User | undefined, token?:string | undefined) {  
  var isLoggedIn = s == LoginStateEnum.logged_in;

  state.user = user;
  state.state = s;
  state.isLoggedIn = isLoggedIn;  
  state.hasAuthenticationError = false;
  state.message = undefined;

  if (isLoggedIn){
    if (token !== null && token !== undefined)
      browserStorage.session.set('token', token);
  } else 
    browserStorage.session.remove('token');
}

function UpdateStateAuthenticationError(state: LoginState, message: string){
  UpdateState(state, LoginStateEnum.logged_out);
  /* Store information in local session storage.
   * The reload will flush all redux data... 
   * At lead time this is retrieved again from sessions storage
   * and placed in redux
   */
  browserStorage.session.set("hasAuthenticationError", true, true);
  browserStorage.session.set("message", message, true);
  window.location.reload();
}

interface Response {
  status: number,
  statusText: string,
  message: string  
}

interface LoginResponse extends Response {
  token: string,
  user: User
}

interface User {
  id: string,
  username: string,
  name: string
}

export const loginAsync = createAsyncThunk<
      LoginResponse, 
      {username: string, secret: string, doRemember: boolean},
      {rejectValue: Response}
    >(
    'authentication/login',
    async ({username, secret, doRemember}: {username: string, secret: string, doRemember: boolean}, /* thunkApi */ { rejectWithValue }) => {
      try{
        if (doRemember) {
          browserStorage.local.set("rememberme", true, true);
          browserStorage.local.set("username", username, true);
        } else {
          browserStorage.local.remove("rememberme");
          browserStorage.local.remove("username");
        }
        var data = {username: username, password: secret};
        var cfg = undefined;
        var response = await axios.post<LoginResponse>('http://127.0.0.1:5000/api/users/authenticate', data, cfg);
        return response.data;
      }catch(err){        
        return rejectWithValue(
          {
            status: err.response.status, 
            statusText: err.response.statusText, 
            message: 'oeps'
          });
      }
    }
  );

export const logoutAsync = createAsyncThunk(
    'authentication/logout',
    async () => {
      const response = await logout();
      return response.data;
    }
  );

export const pingAsync = createAsyncThunk<
      LoginResponse, 
      undefined,
      {rejectValue: Response}
    >(
    'authentication/ping',
    async (undefined, /* thunkApi */ { rejectWithValue }) => {
      try{
        var cfg = undefined;
        var response = await axios.get<LoginResponse>('http://127.0.0.1:5000/api/users/ping', cfg);
        return response.data;
      }catch(err){        
        return rejectWithValue(
          {
            status: err.response.status, 
            statusText: err.response.statusText, 
            message: 'oeps'
          });
      }
    }
  );

export const authenticationSlice = createSlice({
    name: 'authentication',
    initialState,
    reducers: {
      relogin(state) {
        UpdateStateAuthenticationError(state, "There was an authentication error. Please login."); 
      }
    },
    extraReducers: (builder) => {
      builder
        .addCase(loginAsync.pending, (state) => {
          UpdateState(state, LoginStateEnum.log_in, undefined, undefined);
          console.info(`loginAsync.pending`);                    
        })
        .addCase(loginAsync.fulfilled, (state, action) => {                    
          UpdateState(
            state, 
            LoginStateEnum.logged_in, 
            {
              id:action.payload.user.id, 
              username: action.payload.user.username, 
              name: action.payload.user.name 
            }, 
            action.payload.token); 
          console.info(`loginAsync.fulfilled - ${action.payload.user.id} - ${action.payload.user.name}`);  
        })
        .addCase(loginAsync.rejected, (state, action ) => {
          UpdateState(state, LoginStateEnum.logged_out);     
          console.info(`login rejected - ${action.payload?.status} - ${action.payload?.statusText}`);  
        })        
        .addCase(logoutAsync.fulfilled, (state, action) => {
          UpdateState(state, LoginStateEnum.logged_out);
          // force reload of screen to clear the redux state...
          window.location.reload();
          console.info(`logged_out - ${JSON.stringify(action.payload)}`);  
        })
        .addCase(pingAsync.fulfilled, (state, action) =>  {          
          UpdateState(
            state, 
            LoginStateEnum.logged_in, 
            {
              id:action.payload.user.id, 
              username: action.payload.user.username, 
              name: action.payload.user.name 
            }
          ); 
        })
        .addCase(pingAsync.rejected, (state, action ) => {
          UpdateState(state, LoginStateEnum.logged_out);     
          console.info(`pong rejected - ${action.payload?.status} - ${action.payload?.statusText}`);  
        })
        ;
    },
  });

export default authenticationSlice.reducer;      
export const { relogin } = authenticationSlice.actions;

// some selectors to access data from the store that is managed by this slice
export const selectIsLoggedIn = (state : RootState) => state.authentication.isLoggedIn;
export const selectHasAuthenticationError = (state : RootState) => state.authentication.hasAuthenticationError;
export const selectMessage = (state : RootState) => state.authentication.message;

