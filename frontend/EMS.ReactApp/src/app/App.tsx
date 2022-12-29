import { useEffect, useState  } from 'react';
import { styled } from '@mui/material/styles';
import { useTheme } from '@mui/material/styles';
import { Routes, Route } from 'react-router-dom';

import CssBaseline from '@mui/material/CssBaseline';

import useMediaQuery from '@mui/material/useMediaQuery';


import SplashScreen from '../components/splash/SplashScreen';

import MyThemeProvider from '../features/themeprovider/CustomThemeProvider';

import AppDrawer from '../features/appdrawer/drawer';

import AppHeader from '../features/appheader/AppHeader';
import Main from '../features/main/Main';
import Login from '../features/authentication/Login';
import Logout from '../features/authentication/Logout';

import { DrawerDefinition } from '../features/appdrawer/drawer';

import { useAppSelector, useAppDispatch } from '../common/hooks';
import { pingAsync, selectIsLoggedIn } from '../features/authentication/authenticationSlice';
import CheckAuthentication from '../features/authentication/CheckAuthentication';

const PREFIX = 'App';
const classes = {
  content: `${PREFIX}-content`,
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.content}`]: {
    flexGrow: 1,
    paddingLeft: theme.spacing(3) + 56, //(p.isScreenXS ? 0 : 56),
    paddingTop: theme.spacing(3),
    paddingRight: theme.spacing(3),
    paddingBottom: theme.spacing(3)  
  },     
}))

function App() {   
  const [initialized, setInitialized] = useState(false);
  const [initializing, setInitializing] = useState(false);
  const theme = useTheme();

  const isLoggedIn = useAppSelector(selectIsLoggedIn);    
  const isScreenXS = useMediaQuery(theme.breakpoints.only('xs')); 
  
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

  const routesFromDrawer = DrawerDefinition.items.map((item, i) => (
    <Route key={item.key} path={item.route} element= {item.component} />
  ));

  return (
    <Root>
      <MyThemeProvider>
        <CssBaseline />      
        { (!initialized || initializing) && <SplashScreen /> }
        
        <CheckAuthentication/>
        <AppHeader ></AppHeader>    
        <AppDrawer persistent={isLoggedIn && isScreenXS}/>
        <main className={classes.content} > 
          <Routes> 
            <Route path='/' element={<Main/>} />
            <Route path='/' element={<Main/>} />
            <Route path='/login' element={<Login/>} />
            <Route path='/logout' element={<Logout/>} />
            {routesFromDrawer} 
          </Routes> 
        </main>
      </MyThemeProvider>
    </Root>
  );
}

export default App;
