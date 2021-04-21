import React from 'react';
import { BrowserRouter as BrowserRouter, Route, Switch, Link } from 'react-router-dom';
import logo from './logo.svg';
import './App.css';
import Main from '../components/Main/Main';
import Login from '../components/Login/Login';

function App() {
  return (
    <div className="App">
      <BrowserRouter basename="/app">
          <Link to={'/dashboard'}>dashboard</Link>
          <Route path='/' render={() => (<h1>Welcome ;-)</h1>)}></Route>
          <Route path='/dashboard' render={() => (<h1>Welcome ***</h1>)}></Route>
          <Route path='/login'> <Login/> </Route>      
      </BrowserRouter>
    </div>
  );
}

export default App;
