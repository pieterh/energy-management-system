import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { login, logout } from './authenticationAPI';

export interface LoginState {
    value: number;
    state: 'logged_out' | 'log_in' | 'logged_in' | 'log_out' ;
  }
  
  const initialState: LoginState = {
    value: 0,
    state: 'logged_out',
  };
  
  export const loginAsync = createAsyncThunk<any, {username: string, secret: string}>(
    'authentication/login',
    async ({username, secret}: {username: string, secret: string}, /* thunkApi */ { rejectWithValue }) => {
      try{
        console.info(`login(${username}, ${secret}) ->`);   
        const response = await login(username, secret);
        console.info("login <- "+ JSON.stringify(response.data));  
        return response.data;
      }catch(err){
        var resp = {ok: false, message: 'oeps'};
        return rejectWithValue(err);
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


export const authenticationSlice = createSlice({
    name: 'authentication',
    initialState,
    reducers: {
      increment: (state) => {
        console.log("increment...");
      },
    },
    // The `extraReducers` field lets the slice handle actions defined elsewhere,
    // including actions generated by createAsyncThunk or in other slices.
    extraReducers: (builder) => {
      builder
        .addCase(loginAsync.pending, (state) => {
          state.state = 'log_in';
          console.info(`loginAsync.pending - ${state.state} `);                    
        })
        .addCase(loginAsync.fulfilled, (state, action) => {
          state.state = 'logged_in';
          console.info(`loginAsync.fulfilled - ${state.state} - ${JSON.stringify(action.payload)}`);  
        })
        .addCase(loginAsync.rejected, (state, action) => {
          state.state = 'logged_out';
          console.info(`login rejected - ${state.state} - ${JSON.stringify(action.payload)}`);  
        })        
        .addCase(logoutAsync.pending, (state) => {
          state.state = 'log_out';
        })
        .addCase(logoutAsync.fulfilled, (state, action) => {
          state.state = 'logged_out';
          console.info(`logged_out - ${state.state} - ${JSON.stringify(action.payload)}`);  
        })
        ;
    },
  });

export const { increment } = authenticationSlice.actions;  

export default authenticationSlice.reducer;      
