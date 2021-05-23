import { combineReducers } from 'redux';
import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit'
import { connectRouter, routerMiddleware } from 'connected-react-router'
import { createBrowserHistory, History } from 'history'

import drawerReducer from '../features/appdrawer/drawerSlice';
import authenticationReducer from '../features/authentication/authenticationSlice';
import hemsReducer from '../features/hems/hemsSlice';
import evseReducer from '../features/chargepoint/EVSESlice';
import customThemeProviderReducer from '../features/themeprovider/CustomThemeProviderSlice';

export const history = createBrowserHistory();

const createRootReducer = (history: History) => combineReducers({
  router: connectRouter(history),
  drawer: drawerReducer,
  authentication: authenticationReducer,
  hems: hemsReducer,
  evse: evseReducer,
  customTheme: customThemeProviderReducer
});

export const store = configureStore({
  reducer: createRootReducer(history),        
  middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(routerMiddleware(history)),
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
