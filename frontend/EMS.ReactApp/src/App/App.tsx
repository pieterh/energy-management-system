import React from 'react';
import { connect } from "react-redux"

import { createStore } from 'redux';
import { Provider } from "react-redux";
import { BrowserRouter as BrowserRouter, Route, Switch, Link, Redirect } from 'react-router-dom';
import logo from './logo.svg';
import './App.css';
import Main from '../components/Main/Main';
import AppHeader from '../components/AppHeader/AppHeader';
import Login from '../components/Login/Login';
import { store, RootState } from './store';

import { useDispatch, useSelector } from 'react-redux';

function App() {
  return (
    <div className="App">
      <Provider store={store}>    
        <BrowserRouter basename="/app">
            <AppHeader />
            <Route path='/dashboard' render={() => (<h1>Welcome ***</h1>)}></Route>
            <Route path='/login'> <Login/> </Route>                   
        </BrowserRouter>
      </Provider> 
    </div>
  );
}

export default App;
