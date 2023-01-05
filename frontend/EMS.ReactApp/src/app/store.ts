import { combineReducers } from 'redux';
import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit'
import { createReduxHistoryContext, reachify } from "redux-first-history";
import { createBrowserHistory } from 'history'

import drawerReducer from '../features/appdrawer/drawerSlice';
import authenticationReducer from '../features/authentication/authenticationSlice';
import hemsReducer from '../features/hems/hemsSlice';
import evseReducer from '../features/chargepoint/EVSESlice';
import smartmeterReducer from '../features/smartmeter/smartmeterSlice';
import customThemeProviderReducer from '../features/themeprovider/CustomThemeProviderSlice';

const { createReduxHistory, routerMiddleware, routerReducer } = createReduxHistoryContext({ 
  history: createBrowserHistory(),
  //other options if needed 
});

const createRootReducer = () => combineReducers({
  router: routerReducer,
  drawer: drawerReducer,
  authentication: authenticationReducer,
  hems: hemsReducer,
  evse: evseReducer,
  smartmeter: smartmeterReducer,
  customTheme: customThemeProviderReducer
});

export const store = configureStore({
  reducer: createRootReducer(),        
  middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(routerMiddleware),
})

export const history = createReduxHistory(store);

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
