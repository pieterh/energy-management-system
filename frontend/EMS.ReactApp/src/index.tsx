import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './app/App';
import { Provider } from "react-redux";
import { store } from './app/store';


ReactDOM.render(
  // Strict mode is not working properly with material-ui (in development mode)
  // ie. toggle theme creates each time a set of styles in the header
  // <React.StrictMode>
  <Provider store={store}>
      <App />
   </Provider>,
  // </React.StrictMode>,
  document.getElementById('root')
);


// this interceptor is placed here so that it has access
// to the store outside of the components/functions
// and accesses the store after initialization
// so we can't put it in the authenticationSlice....
import axios from 'axios';
import { relogin } from './features/authentication/authenticationSlice';

axios.interceptors.response.use(function (response) {
  return response;
}, function (error) {
  if (error.response.status === 401) {
    try{      
      store.dispatch(relogin());
    }catch(error){
      console.log(error);
    }
  } else {
      return Promise.reject(error);
  }
});