import React from 'react';
import { createMuiTheme, Theme, useTheme } from '@material-ui/core/styles';
import { ThemeProvider } from '@material-ui/core/styles';

import { RootState, store } from './store';
import { ThemeTypes } from './CustomThemeProviderSlice';
import { useSelector } from 'react-redux';

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

const availableThemes : {[key: string]: Theme} = {
  0: lightTheme,
  1: darkTheme
};

export function MyThemeProvider({ children } : ThemeProviderProps) {
  const themeName = useSelector<RootState>( state => state.customTheme.themeName ) as string;
  const themeType = useSelector<RootState>( state => state.customTheme.themeType ) as ThemeTypes;
  const currentTheme = availableThemes[themeType];

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


// const themeObject : ThemeOptions = {
//   palette: {
//     primary: { main: "#053f5b" },
//     secondary: { main: "#5e3c6f" },
//     type: "light"
//   },
//   // themeName: "Blue Lagoon 2020",
//   typography: {
//     fontFamily: "Bitter"
//   }
// };

// const useDarkMode = () => {
//   const [theme1, setTheme] = useState(themeObject);

//   let theme : ThemeOptions = theme1;

//   const {
//     palette: { type }
//   } = theme;
//   const toggleDarkMode = () => {
//     const updatedTheme = {
//       ...theme,
//       palette: {
//         ...theme.palette,
//         type: type === "light" ? "dark" : "light"
//       }
//     };
//     setTheme(updatedTheme);
//   };
//   return [theme, toggleDarkMode];
// };