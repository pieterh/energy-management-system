import { useEffect } from 'react';
import { useHistory, useLocation } from 'react-router-dom';

import { useAppSelector } from  '../../common/hooks';
import { selectHasAuthenticationError, selectIsLoggedIn } from './authenticationSlice';


export default function CheckAuthentication() {  
  const hasAuthenticationError = useAppSelector(selectHasAuthenticationError);
  const history = useHistory();
  const pathname = useLocation().pathname;
  const isLoggedIn = useAppSelector(selectIsLoggedIn);  

  useEffect(() => {  
    if (hasAuthenticationError || (!isLoggedIn && pathname !== "/" && pathname !== "/login")){
      console.log("Redirect to login");
      history.push('/login');
    }
  }, [hasAuthenticationError, pathname, isLoggedIn]);
  return(<></>);
}
