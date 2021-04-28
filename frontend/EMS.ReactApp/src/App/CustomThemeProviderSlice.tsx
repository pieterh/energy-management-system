import { createAsyncThunk, createSlice,  createAction, PayloadAction } from '@reduxjs/toolkit';

export enum ThemeTypes { light, dark };

export interface CustomThemeState {
    themeType: ThemeTypes;
    themeName: string;
  }

const initialState: CustomThemeState = {
    themeType: ThemeTypes.light,
    themeName: 'light'
  };

export const ChangeTheme = createAction<ThemeTypes>('ChangeTheme');

export const CustomThemeProviderSlice = createSlice({
    name: 'CustomTheme',
    initialState,
    reducers: {
    },
    extraReducers: builder => {
        builder.addCase(ChangeTheme, (state, action) => {
            console.log("change theme...");
            state.themeType = action.payload;
            state.themeName = state.themeType ==  ThemeTypes.light ? 'light' : 'dark';
          })
    }
  });

export default CustomThemeProviderSlice.reducer;
