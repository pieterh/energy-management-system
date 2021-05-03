import { useEffect } from 'react';
import { useHistory } from 'react-router-dom';

import { useAppSelector } from  '../../common/hooks';
import { selectHasAuthenticationError } from './authenticationSlice';

export default function CheckAuthentication() {  
  const hasAuthenticationError = useAppSelector(selectHasAuthenticationError);
  const history = useHistory();
  useEffect(() => {  
    if (hasAuthenticationError){
      console.log("Redirect to login");
      history.push('/login');
    }
  }, [hasAuthenticationError]);
  return(<></>);
}
