import { useEffect, useState  } from 'react';
import { BrowserRouter as BrowserRouter, Route } from 'react-router-dom';

import CssBaseline from '@material-ui/core/CssBaseline';

import { useAppDispatch } from '../common/hooks';
import SplashScreen from '../components/splash/SplashScreen';

import { pingAsync } from '../features/authentication/authenticationSlice';
import MyThemeProvider from '../features/themeprovider/CustomThemeProvider';

import AppHeader from '../features/appheader/AppHeader';
import Main from '../features/main/Main';
import Login from '../features/authentication/Login';
import Logout from '../features/authentication/Logout';

import CheckAuthentication from '../features/authentication/CheckAuthentication';

function App() {   
  const [initialized, setInitialized] = useState(false);
  const [initializing, setInitializing] = useState(false);

  
  const dispatch = useAppDispatch();

  useEffect(() => {    
     if (!initialized && !initializing) {
        setInitializing(true);
        // we use ping to check if the user is authenticated or not
        // and the redux store is updated accordingly
        dispatch(pingAsync()).then(() =>  {
          setInitialized(true); 
          setInitializing(false);
        });
      }
  },[initialized, initializing]);

  return (
    <MyThemeProvider>
      <CssBaseline />
      <BrowserRouter basename="/app">                   
            { (!initialized || initializing) && <SplashScreen /> }
            <CheckAuthentication/>
            <AppHeader >
              <Route path='/' exact> <Main/> </Route>
              <Route path='/login'> <Login/> </Route>
              <Route path='/logout'> <Logout/> </Route>
            </AppHeader>          
      </BrowserRouter>
    </MyThemeProvider>
  );
}

export default App;
