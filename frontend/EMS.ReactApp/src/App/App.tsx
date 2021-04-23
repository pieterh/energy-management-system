import React from 'react';
import { createStore } from 'redux';
import { Provider } from "react-redux";
import { BrowserRouter as BrowserRouter, Route, Switch, Link, Redirect } from 'react-router-dom';
import logo from './logo.svg';
import './App.css';
import Main from '../components/Main/Main';
import Login from '../components/Login/Login';
import {store} from './store';

import { useDispatch, useSelector } from 'react-redux';

function App() {
//  const user = useSelector(state => state. . .authentication.user);
  //const store = createStore(rootReducer);
  return (
    <div className="App">
      <Provider store={store}>
        <BrowserRouter basename="/app">
            <Link to={'/dashboard'}>dashboard</Link>
            <Route path='/' render={() => (<h1>Welcome ;-)</h1>)}></Route>
            <Route path='/dashboard' render={() => (<h1>Welcome ***</h1>)}></Route>
            <Route path='/login'> <Login/> </Route>      

        </BrowserRouter>
      </Provider>
    </div>
  );
}

export default App;
