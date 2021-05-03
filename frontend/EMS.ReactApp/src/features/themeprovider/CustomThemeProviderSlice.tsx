import { createAsyncThunk, createSlice,  createAction, PayloadAction } from '@reduxjs/toolkit';
import  browserStorage  from 'store2';

export enum ThemeTypes { light = 0, dark = 1, device= 2 };

export interface CustomThemeState {
    themeType: ThemeTypes;
    themeName: string;
  }

function GetInitialTheme() : ThemeTypes {
  return browserStorage.local.has("theme") ? browserStorage.local.get("theme") : ThemeTypes.device;
}

function GetThemeName(t : ThemeTypes) : string {
  switch(t){
    case ThemeTypes.light:
      return 'Light theme';
    case ThemeTypes.dark:
      return 'Dark theme';      
    case ThemeTypes.device:
      return 'Device theme';      
  }
}

const initialState: CustomThemeState = {
    themeType: GetInitialTheme(),
    themeName: GetThemeName(GetInitialTheme())
  };

export const ChangeTheme = createAction<ThemeTypes>('ChangeTheme');

export const CustomThemeProviderSlice = createSlice({
    name: 'CustomTheme',
    initialState,
    reducers: {
    },
    extraReducers: builder => {
        builder.addCase(ChangeTheme, (state, action) => {
            console.log(`change theme... ${state.themeType}`);
            state.themeType = action.payload;
            state.themeName = GetThemeName(state.themeType);
            if (browserStorage.local.has("rememberme")){
              browserStorage.local.set("theme", state.themeType, true);
            }
          })
    }
  });

export default CustomThemeProviderSlice.reducer;
