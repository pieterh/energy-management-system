import React, { useState } from 'react';
import { Navigate } from 'react-router-dom';
import Container from '@mui/material/Container';
import Button from '@mui/material/Button';

import { logoutAsync } from './authenticationSlice';

import { useAppSelector, useAppDispatch } from  '../../app/hooks';
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
  if (!isLoggedIn) { return (<Navigate to='/'/>); } else
  return (
      <Container maxWidth="xs"> 
        <Button onClick={(event) => onLogoutClick(event)} color="inherit">Logout</Button>
      </Container>  
  );
}

export default Logout;
