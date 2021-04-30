import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App/App';
import { Provider } from "react-redux";
import { store } from './App/store';
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
