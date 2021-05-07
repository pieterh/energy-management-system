import React from 'react';

import { useLocation, useHistory } from 'react-router-dom';

import MuiIconButton from '@material-ui/core/IconButton';
import { makeStyles, Theme } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import IconButton from '@material-ui/core/IconButton';
import { default as MenuIcon } from '@material-ui/icons/Menu';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';
import WbSunnyTwoToneIcon from '@material-ui/icons/WbSunnyTwoTone';
import NightsStayTwoToneIcon from '@material-ui/icons/NightsStayTwoTone';

import PersonIcon from '@material-ui/icons/Person';
import ClickAwayListener from '@material-ui/core/ClickAwayListener';
import Grow from '@material-ui/core/Grow';
import Paper from '@material-ui/core/Paper';
import Popper from '@material-ui/core/Popper';
import MenuItem from '@material-ui/core/MenuItem';
import MenuList from '@material-ui/core/MenuList';
import {green, yellow, red }from "@material-ui/core/colors";

import { useAppSelector, useAppDispatch } from '../../App/hooks';
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
    offset: theme.mixins.toolbar,
    lightbutton:{
      color: yellow[500]
    },
    darkbutton:{
      color: yellow[500]
    },
    personbutton:{
      color: red[500]
    }        
  }));

  
export function AppHeader(){           
    const dispatch = useAppDispatch();
    const [state, setState] = React.useState({
      darkState: false
    });
    
    const buttonRef = React.useRef(null);
    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);

    const classes = useStyles(); 
    const location = useLocation();  
    const history = useHistory();

    const isLoggedIn = useAppSelector( state => state.authentication.isLoggedIn ) as boolean;

    const showLoginButton = !isLoggedIn && location.pathname != '/login' && location.pathname != '/logout';
    const showLogoutButton = isLoggedIn && location.pathname != '/login' && location.pathname != '/logout';

    var title = "";
    title = location.pathname == '/' ? 'Main' : title;
    title = location.pathname == '/login' ? 'Login' : title;
    title = location.pathname == '/logout' ? 'Logout' : title;
    title = location.pathname == '/dashboard' ? 'Dashboard' : title;
    title = location.pathname == '/settings' ? 'Settings' : title;

    const handleToggle = () => {
      setAnchorEl(buttonRef.current);
    };
  
    const handleClose = (event: React.MouseEvent<{}>) => {
      setAnchorEl(null);
    };
  
    function handleListKeyDown(event: React.KeyboardEvent<HTMLUListElement>) {
      if (event.key === 'Tab') {
        event.preventDefault();
        setAnchorEl(null);
      }
    }
  
    function onLogoutClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent> | null)  {
      setAnchorEl(null);
      history.push('/logout');
    }

    function onLoginClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent> | null)  {
      setAnchorEl(null);
      history.push('/login');
    }

    function onThemeChange(event: React.ChangeEvent<HTMLInputElement>)  {
       dispatch(ChangeTheme(event.target.checked ? ThemeTypes.dark : ThemeTypes.light));
    }

    const handleThemeChange = (event:any) => {
      setState({ ...state, darkState: !state.darkState });
      dispatch(ChangeTheme(!state.darkState? ThemeTypes.dark : ThemeTypes.light));
    };

    return (
      <AppBar color="inherit" position="static">
        <Toolbar>
          <IconButton disabled={!isLoggedIn} edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
          <MenuIcon />
          </IconButton>
          <Typography variant="h6" className={classes.title}>
          {title}
          </Typography>
          { (showLoginButton || showLogoutButton) && 
            <MuiIconButton 
                            ref={buttonRef} aria-controls={open ? 'menu-list-grow' : undefined}
                            aria-haspopup="true"
                            onClick={handleToggle}> 
              <PersonIcon/> 
            </MuiIconButton> }          
          <Popper open={open} anchorEl={anchorEl} role={undefined} transition disablePortal>
          {({ TransitionProps, placement }) => (
            <Grow
              {...TransitionProps}
              style={{ transformOrigin: placement === 'bottom' ? 'center top' : 'center bottom' }}
            >
              <Paper>
                <ClickAwayListener onClickAway={handleClose}>
                  <MenuList autoFocusItem={open} id="menu-list-grow" onKeyDown={handleListKeyDown}>                             
                    { !showLogoutButton && <MenuItem onClick={(event) => onLoginClick(null)}>Login</MenuItem> }
                    { showLogoutButton && <div>
                      <MenuItem onClick={handleClose}>Profile</MenuItem>
                      <MenuItem onClick={handleClose}>My account</MenuItem>                    
                      <MenuItem onClick={(event) => onLogoutClick(null)}>Logout</MenuItem> </div>
                    }
                  </MenuList>
                </ClickAwayListener>
              </Paper>
            </Grow>
          )}
        </Popper>

          { !state.darkState && <MuiIconButton onClick={handleThemeChange}> <NightsStayTwoToneIcon/> </MuiIconButton> }
          { state.darkState && <MuiIconButton onClick={handleThemeChange} > <WbSunnyTwoToneIcon/> </MuiIconButton> }        
        </Toolbar>
      </AppBar> 
    );
  }

export default AppHeader;