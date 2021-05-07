import React, { useState } from 'react';
import { Redirect } from 'react-router-dom';
import Container from '@material-ui/core/Container';
import Button from '@material-ui/core/Button';

import { logoutAsync } from './authenticationSlice';

import { useAppSelector, useAppDispatch } from '../../App/hooks';

export function Logout() {
  const isLoggedIn = useAppSelector( state => state.authentication.isLoggedIn ) as boolean;
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
      <Container component="main" maxWidth="xs"> 
      <Button onClick={(event) => onLogoutClick(event)} color="inherit">Logout</Button>
      </Container>  
  );
}

export default Logout;
