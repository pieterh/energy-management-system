import React from 'react';
import { createMuiTheme, Theme, useTheme } from '@material-ui/core/styles';
import { ThemeProvider } from '@material-ui/core/styles';
import useMediaQuery from "@material-ui/core/useMediaQuery";

import { useAppSelector } from '../../App/hooks'

import { ThemeTypes } from './CustomThemeProviderSlice';

interface ThemeProviderProps {
  children: React.ReactNode
}

type ThemeContext = {
  currentTheme: string;
};

const CustomThemeContext = React.createContext<ThemeContext>({ currentTheme: '--' });
CustomThemeContext.displayName = 'CustomThemeContext';

const darkTheme = createMuiTheme({
  // ...theme,
  palette: {
    type: 'dark'
  }
});

const lightTheme = createMuiTheme({
  // ...theme,
  palette: {
    type: 'light' 
  }
});

export function MyThemeProvider({ children } : ThemeProviderProps) {
  const themeName = useAppSelector( state => state.customTheme.themeName ) as string;
  const themeType = useAppSelector( state => state.customTheme.themeType ) as ThemeTypes;  
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)');  
  var currentTheme : Theme;
  switch(themeType){
    case ThemeTypes.light:
      currentTheme = lightTheme;
      break;
    case ThemeTypes.dark:
      currentTheme = darkTheme;
      break;      
    case ThemeTypes.device:      
      currentTheme = prefersDarkMode ? darkTheme: lightTheme;
      break;      
  }
  
  const contextValue = {
    currentTheme: themeName
  }
  
  return (
    <CustomThemeContext.Provider value={contextValue}>
      <ThemeProvider theme={currentTheme}>{children}</ThemeProvider>
    </CustomThemeContext.Provider>
  )
}

export default MyThemeProvider;
