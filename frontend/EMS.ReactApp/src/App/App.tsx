import { Provider } from "react-redux";
import { BrowserRouter as BrowserRouter, Route } from 'react-router-dom';
import { store } from './store';

import './App.css';

import AppHeader from '../components/AppHeader/AppHeader';
import Login from '../components/Login/Login';
import Logout from '../components/Logout/Logout';

function App() {

  return (
    <div className="App">
      <Provider store={store}>    
        <BrowserRouter basename="/app">
            <AppHeader />
            <Route path='/dashboard' render={() => (<h1>Welcome ***</h1>)}></Route>
            <Route path='/login'> <Login/> </Route>                   
            <Route path='/logout'> <Logout/> </Route> 
        </BrowserRouter>
      </Provider> 
    </div>
  );
}

export default App;
