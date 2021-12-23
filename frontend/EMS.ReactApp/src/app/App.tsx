import { useEffect, useState  } from 'react';
import { Routes, Route } from 'react-router-dom';

import CssBaseline from '@material-ui/core/CssBaseline';

import useMediaQuery from '@material-ui/core/useMediaQuery';
import { makeStyles, Theme, useTheme } from '@material-ui/core/styles';

import SplashScreen from '../components/splash/SplashScreen';

import { pingAsync } from '../features/authentication/authenticationSlice';
import MyThemeProvider from '../features/themeprovider/CustomThemeProvider';

import AppDrawer from '../features/appdrawer/drawer';

import AppHeader from '../features/appheader/AppHeader';
import Main from '../features/main/Main';
import Login from '../features/authentication/Login';
import Logout from '../features/authentication/Logout';

import { DrawerDefinition } from '../features/appdrawer/drawer';

import { useAppSelector, useAppDispatch } from '../common/hooks';
import { selectIsLoggedIn } from '../features/authentication/authenticationSlice';
import CheckAuthentication from '../features/authentication/CheckAuthentication';


const useStyles = makeStyles((theme: Theme) =>({
  content: () => ({ 
    flexGrow: 1,
    paddingLeft: theme.spacing(3) + 56, //(p.isScreenXS ? 0 : 56),
    paddingTop: theme.spacing(3),
    paddingRight: theme.spacing(3),
    paddingBottom: theme.spacing(3)    
  }),
}));

function App() {   
  const [initialized, setInitialized] = useState(false);
  const [initializing, setInitializing] = useState(false);

  const theme = useTheme();

  const isLoggedIn = useAppSelector(selectIsLoggedIn);    
  const isScreenXS = useMediaQuery(theme.breakpoints.only('xs'));
  const classes = useStyles({isScreenXS: isScreenXS});   

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
  );
}

export default App;
