import React, { useState } from 'react';
import { Redirect } from 'react-router-dom';
import Container from '@material-ui/core/Container';
import Button from '@material-ui/core/Button';

import { logoutAsync, isLoggedIn } from '../../App/authenticationSlice';

import { useAppSelector, useAppDispatch } from '../../App/hooks';

export function Logout() {
    const loggedIn = useAppSelector(isLoggedIn);
    const dispatch = useAppDispatch()
    const [isBusy, setIsBusy] = useState(false);

    function onLogoutClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent>)  {
        console.log(`onLogoutClick -> `);
        setIsBusy(true);    
        dispatch(logoutAsync()).then((x) =>{
          console.log("onLogoutClick <-");
        }).finally(() =>{
          setIsBusy(false); 
          console.log("onLogoutClick <- finally");
        });  
    }
    if (!loggedIn) { return (<Redirect to='/'/>); } else
    return (
        <Container component="main" maxWidth="xs"> 
        <Button onClick={(event) => onLogoutClick(event)} color="inherit">Logout</Button>
        </Container>  
    );
}

export default Logout;
