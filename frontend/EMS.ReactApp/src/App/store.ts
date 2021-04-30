import { combineReducers } from 'redux';
import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit'
import { useDispatch } from 'react-redux'

import authenticationReducer from './authenticationSlice';
import CustomThemeProviderReducer from './CustomThemeProviderSlice';

const rootReducer = combineReducers({
    authentication: authenticationReducer,
    customTheme: CustomThemeProviderReducer
  });

export const store = configureStore({
  reducer: rootReducer,    
})

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>

// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = typeof store.dispatch
export const useAppDispatch = () => useDispatch<AppDispatch>() // Export a hook that can be reused to resolve types

export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
