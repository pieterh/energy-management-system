import { useEffect, useState  } from 'react';
import { BrowserRouter as BrowserRouter, Route } from 'react-router-dom';

import CssBaseline from '@material-ui/core/CssBaseline';
import useMediaQuery from "@material-ui/core/useMediaQuery";

//import './App.css';
import MyThemeProvider from './CustomThemeProvider';
import { useAppDispatch } from './hooks';
import { pingAsync } from './authenticationSlice';

import AppHeader from '../components/AppHeader/AppHeader';
import Main from '../components/Main/Main';
import Login from '../components/Login/Login';
import Logout from '../components/Logout/Logout';

function App() {   
  const [initialized, setInitialized] = useState(false);
  const [initializing, setInitializing] = useState(false);

  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)');
  const dispatch = useAppDispatch();

  useEffect(() => {    
     console.log(`prefers darkmode? -> ${prefersDarkMode}`);
     if (!initialized && !initializing){
        setInitializing(true);
        dispatch(pingAsync()).then(() =>{
          setInitialized(true);
          setInitializing(false);
        });
      }
  },[prefersDarkMode, initialized, initializing]);

  return (
    <MyThemeProvider>
      <CssBaseline />
      <BrowserRouter basename="/app">
          <AppHeader />
          <Route path='/' exact> <Main/> </Route>
          <Route path='/login'> <Login/> </Route>                   
          <Route path='/logout'> <Logout/> </Route> 
      </BrowserRouter>
    </MyThemeProvider>
  );
}

export default App;
