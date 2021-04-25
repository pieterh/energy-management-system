
import React, { MouseEventHandler } from 'react';
import { Provider } from "react-redux";
import { useLocation, useHistory } from 'react-router-dom';
import { makeStyles, Theme } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import IconButton from '@material-ui/core/IconButton';
import { default as MenuIcon } from '@material-ui/icons/Menu';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';

import green from "@material-ui/core/colors/green";

import { store, RootState } from '../../App/store';

import { useAppSelector, useAppDispatch } from '../../App/hooks';

import { isLoggedIn } from '../../App/authenticationSlice';

interface IAppHeaderProps {
}
  
interface IAppHeaderState {
}

//const authentication = useSelector((state: RootState) => state.authentication);

const useStyles = makeStyles((theme: Theme) =>({
    menuButton: {
      marginRight: theme.spacing(2)
    },
    title: {
      flexGrow: 1
    },
    customColor: {
      // or hex code, this is normal CSS background-color
      backgroundColor: green[500]
    },
    customHeight: {
      minHeight: 200
    },
    offset: theme.mixins.toolbar
  }));

export function AppHeader(){       
    const classes = useStyles(); 
    const location = useLocation();  
    const history = useHistory();

    const loggedIn = useAppSelector(isLoggedIn);
    const showLoginButton = !loggedIn && location.pathname != '/login' && location.pathname != '/logout';
    const showLogoutButton = loggedIn && location.pathname != '/login' && location.pathname != '/logout';

    function onLogoutClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent>)  {
      history.push('/logout');
    }

    function onLoginClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent>)  {
      history.push('/login');
    }

    return (
        <Provider store={store}>    
            <AppBar position="static">
                <Toolbar>
                    <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
                    <MenuIcon />
                    </IconButton>
                    <Typography variant="h6" className={classes.title}>
                    News
                    </Typography>
                    <div>
                    {showLogoutButton && <Button onClick={(event) => onLogoutClick(event)} color="inherit">Logout</Button> }
                    </div>
                    <div>
                    {showLoginButton && <Button onClick={(event) => onLoginClick(event)}color="inherit">Login</Button> }
                    </div>
                </Toolbar>
            </AppBar> 
        </Provider>
    );
    }

    export default AppHeader;