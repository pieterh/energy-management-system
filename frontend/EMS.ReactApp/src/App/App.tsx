import { Provider } from "react-redux";
import { BrowserRouter as BrowserRouter, Route } from 'react-router-dom';
import CssBaseline from '@material-ui/core/CssBaseline';

import useMediaQuery from "@material-ui/core/useMediaQuery";

import { store } from './store';
import './App.css';

import MyThemeProvider from './CustomThemeProvider';
import AppHeader from '../components/AppHeader/AppHeader';
import Main from '../components/Main/Main';
import Login from '../components/Login/Login';
import Logout from '../components/Logout/Logout';

function App() { 
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)');
  console.log(`prefers darkmode? -> ${prefersDarkMode}`);

  return (
    <div className="App">
      <Provider store={store}>
      <MyThemeProvider>
          <CssBaseline />
          <BrowserRouter basename="/app">
              <AppHeader />
              <Route path='/' exact> <Main/> </Route>
              <Route path='/login'> <Login/> </Route>                   
              <Route path='/logout'> <Logout/> </Route> 
          </BrowserRouter>
        </MyThemeProvider>
      </Provider>
    </div>
  );
}

export default App;
