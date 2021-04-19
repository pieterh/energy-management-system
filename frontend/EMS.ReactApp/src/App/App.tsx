import React from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import logo from './logo.svg';
import './App.css';
import Main from '../components/Main/Main';
import Login from '../components/Login/Login';

function App() {
  return (
    <div className="App">
      <Router>
          <Link to={'/app/dashboard'}>dashbaord</Link>
          <Route path='/' render={() => (<h1>Welcome ;-)</h1>)}></Route>
          <Route path='/app/dashboard' render={() => (<h1>Welcome ...</h1>)}></Route>
          <Route path='/app/login' component='Login'/>        
      </Router>
    </div>
  );
}

export default App;
