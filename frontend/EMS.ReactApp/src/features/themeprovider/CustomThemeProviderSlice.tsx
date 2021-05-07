import { createAsyncThunk, createSlice,  createAction, PayloadAction } from '@reduxjs/toolkit';

export enum ThemeTypes { light = 0, dark = 1, device= 2 };

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
            console.log(`change theme... ${state.themeType}`);
            state.themeType = action.payload;
            switch(action.payload){
              case ThemeTypes.light:
                state.themeName = 'Light theme';
                break;
              case ThemeTypes.dark:
                state.themeName = 'Dark theme';
                break;
              case ThemeTypes.device:
                state.themeName = 'Device theme';
                break;
            }            
          })
    }
  });

export default CustomThemeProviderSlice.reducer;
