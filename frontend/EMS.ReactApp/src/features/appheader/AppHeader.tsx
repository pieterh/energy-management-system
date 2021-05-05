import React from 'react';

import { useLocation, useHistory } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';

import { makeStyles, Theme } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import IconButton from '@material-ui/core/IconButton';
import { default as MenuIcon } from '@material-ui/icons/Menu';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import Switch from "@material-ui/core/Switch";

import green from "@material-ui/core/colors/green";

import { useAppSelector, useAppDispatch } from '../../App/hooks';

import { isLoggedIn } from '../../App/authenticationSlice';
import { ChangeTheme, ThemeTypes } from '../themeprovider/CustomThemeProviderSlice';


type FormInputs = {
  darkState : boolean
};

interface IAppHeaderProps {
}
  
interface IAppHeaderState {
}

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
    const dispatch = useAppDispatch();
    const { control, formState: { errors } } = useForm<FormInputs>(
      {
        defaultValues: { 
          darkState: false
        }
      }
    );

    const classes = useStyles(); 
    const location = useLocation();  
    const history = useHistory();

    const loggedIn = useAppSelector(isLoggedIn);
    const showLoginButton = !loggedIn && location.pathname != '/login' && location.pathname != '/logout';
    const showLogoutButton = loggedIn && location.pathname != '/login' && location.pathname != '/logout';

    var title = "";
    title = location.pathname == '/' ? 'Main' : title;
    title = location.pathname == '/login' ? 'Login' : title;
    title = location.pathname == '/logout' ? 'Logout' : title;
    title = location.pathname == '/dashboard' ? 'Dashboard' : title;
    title = location.pathname == '/settings' ? 'Settings' : title;

    function onLogoutClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent>)  {
      history.push('/logout');
    }

    function onLoginClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent>)  {
      history.push('/login');
    }

    function onThemeChange(event: React.ChangeEvent<HTMLInputElement>)  {
       dispatch(ChangeTheme(event.target.checked ? ThemeTypes.dark : ThemeTypes.light));
    }

    return (
      <AppBar color="inherit" position="static">
        <Toolbar>
          <IconButton disabled={!loggedIn} edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
          <MenuIcon />
          </IconButton>
          <Typography variant="h6" className={classes.title}>
          {title}
          </Typography>
          <div>
          {showLogoutButton && <Button onClick={(event) => onLogoutClick(event)} color="inherit">Logout</Button> }
          </div>
          <div>
          {showLoginButton && <Button onClick={(event) => onLoginClick(event)}color="inherit">Login</Button> }
          </div>                                        
          <Controller
            name="darkState"
            control={control}     
            defaultValue={true}
            render={(props) => {
              return (
                <Switch
                  // color='default' 
                  size="small"
                  onChange={(e) => { props.field.onChange(e.target.checked); onThemeChange(e); }}
                  checked={props.field.value} /* set default value */
                />
              );
            }} 
          />
        </Toolbar>
      </AppBar> 
    );
  }

    export default AppHeader;