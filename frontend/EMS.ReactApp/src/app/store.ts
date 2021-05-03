import { combineReducers } from 'redux';
import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit'
 
import authenticationReducer from '../features/authentication/authenticationSlice';
import evseReducer from '../features/chargepoint/EVSESlice';
import CustomThemeProviderReducer from '../features/themeprovider/CustomThemeProviderSlice';

const rootReducer = combineReducers({
    authentication: authenticationReducer,
    evse: evseReducer,
    customTheme: CustomThemeProviderReducer
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
