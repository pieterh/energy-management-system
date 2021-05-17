import React, { useState } from 'react';
import { Redirect } from 'react-router-dom';
import Container from '@material-ui/core/Container';
import Button from '@material-ui/core/Button';

import { logoutAsync } from './authenticationSlice';

import { useAppSelector, useAppDispatch } from  '../../common/hooks';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';

export function Logout() {
  const isLoggedIn = useAppSelector(selectIsLoggedIn);
  const dispatch = useAppDispatch()
  const [isBusy, setIsBusy] = useState(false);

  function onLogoutClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent>)  {
      setIsBusy(true);    
      dispatch(logoutAsync()).then((x) =>{
      }).catch((e) => {
        setIsBusy(false); 
        console.error(e);
      });  
  }
  if (!isLoggedIn) { return (<Redirect to='/'/>); } else
  return (
      <Container maxWidth="xs"> 
        <Button onClick={(event) => onLogoutClick(event)} color="inherit">Logout</Button>
      </Container>  
  );
}

export default Logout;
