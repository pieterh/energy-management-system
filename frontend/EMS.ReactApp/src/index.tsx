import { createRoot } from 'react-dom/client';
import { Provider } from "react-redux";
import { BrowserRouter } from "react-router-dom";
import { ConnectedRouter } from 'connected-react-router'

import { store } from './app/store';
import App from './app/App';

import { AddInterceptors } from './features/authentication/AxiosInterceptors';

const container = document.getElementById('root')
const root = createRoot(container!); 

// make sure to add the interceptors _after_ creating the store and _before_ rendering anything.
// This will make sure that all http calls can be intercepted while initially drawing the components.
AddInterceptors();

root.render(
  // Strict mode is not working properly with material-ui (in development mode)
  // ie. toggle theme creates each time a set of styles in the header
  // <React.StrictMode>
  <Provider store={store}>
    <BrowserRouter>
      <App />
     </BrowserRouter>
   </Provider>,
  // </React.StrictMode>,  
);
