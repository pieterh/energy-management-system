import { combineReducers } from 'redux';
import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit'

import drawerReducer from '../features/appdrawer/drawerSlice';
import authenticationReducer from '../features/authentication/authenticationSlice';
import evseReducer from '../features/chargepoint/EVSESlice';
import customThemeProviderReducer from '../features/themeprovider/CustomThemeProviderSlice';

const rootReducer = combineReducers({
    drawer: drawerReducer,
    authentication: authenticationReducer,
    evse: evseReducer,
    customTheme: customThemeProviderReducer
  });

export const store = configureStore({
  reducer: rootReducer,    
})

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
