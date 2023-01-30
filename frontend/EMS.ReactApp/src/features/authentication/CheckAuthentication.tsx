import { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

import { useAppSelector } from  '../../app/hooks';
import { selectHasAuthenticationError, selectIsLoggedIn } from './authenticationSlice';


export default function CheckAuthentication() {  
  const hasAuthenticationError = useAppSelector(selectHasAuthenticationError);
  const navigate = useNavigate();
  const pathname = useLocation().pathname;
  const isLoggedIn = useAppSelector(selectIsLoggedIn);  

  useEffect(() => {  
    if (hasAuthenticationError || (!isLoggedIn && pathname !== "/" && pathname !== "/login")){
      console.log("Redirect to login");
      navigate('/login');
    }
  }, [hasAuthenticationError, pathname, isLoggedIn]);
  return(<></>);
}
