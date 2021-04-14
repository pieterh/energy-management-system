import React from 'react';
import { Provider } from "react-redux";
import { store } from '../../App/store';
import Dashboard from '../Dashboard/Dashboard';

export default function Main() {
  return(
    <Provider store={store}>    
      <Dashboard />
    </Provider>
  )
}
