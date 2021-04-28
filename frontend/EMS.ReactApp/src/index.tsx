import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App/App';

ReactDOM.render(
  // Strict mode is not working properly with material-ui
  // ie. toggle theme creates each time a set of styles in the header
  // <React.StrictMode>
      <App />,
  // </React.StrictMode>,
  document.getElementById('root')
);
