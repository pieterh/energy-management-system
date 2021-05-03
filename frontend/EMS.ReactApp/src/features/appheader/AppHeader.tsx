import React from 'react';
import { useLocation } from 'react-router-dom';

import useMediaQuery from '@material-ui/core/useMediaQuery';
import { makeStyles, Theme, useTheme } from '@material-ui/core/styles';

import AppBar from '@material-ui/core/AppBar';
import IconButton from '@material-ui/core/IconButton';
import MenuIcon from '@material-ui/icons/Menu';
import MenuOpenIcon from '@material-ui/icons/MenuOpen';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';

import { useAppSelector, useAppDispatch } from '../../common/hooks';

import { selectIsDrawerOpen, openDrawer, closeDrawer, toggleDrawer } from '../appdrawer/drawerSlice';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';
import AccountMenu from '../accountmenu/AccountMenu';

interface IStyleProps {
  isScreenXS: boolean
}

const useStyles = makeStyles((theme: Theme) =>({
  root: {
    display: 'flex',
  },
  appBar:{
  },
  menuButton: {
    marginRight: theme.spacing(2)
  },
  title: {
    flexGrow: 1
  },
  toolbar: {
    paddingLeft: 16,
  },
}));

type Props = {
  children?: React.ReactNode;
};

export function AppHeader({children}: Props): JSX.Element {           
    const dispatch = useAppDispatch();
    const location = useLocation();  
    const theme = useTheme();

    const isLoggedIn = useAppSelector(selectIsLoggedIn);    
    const isScreenXS = useMediaQuery(theme.breakpoints.only('xs'));
    const classes = useStyles({isScreenXS: isScreenXS});   

    const isDrawerOpen = useAppSelector(selectIsDrawerOpen);
    
    var title = "";
    title = location.pathname == '/' ? 'Main' : title;
    title = location.pathname == '/login' ? 'Login' : title;
    title = location.pathname == '/logout' ? 'Logout' : title;
    title = location.pathname == '/dashboard' ? 'Dashboard' : title;
    title = location.pathname == '/settings' ? 'Settings' : title;

    function onDrawerToggleClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent> | null)  {
      dispatch(toggleDrawer());
    }
  
    function onDrawerCloseClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent> | null)  {
      dispatch(closeDrawer());
    }

    return (
      <React.Fragment>   
        <div className={classes.root}>     
          <AppBar color="inherit" position="static" className={classes.appBar}>
            <Toolbar className={classes.toolbar}>
              <IconButton hidden={isDrawerOpen} disabled={!isLoggedIn} edge="start" 
                          className={classes.menuButton} color="inherit" aria-label="menu"
                          onClick={onDrawerToggleClick}>
                { !isDrawerOpen && <MenuIcon /> }
                { isDrawerOpen && <MenuOpenIcon/> }
              </IconButton>
              <Typography variant="h6" className={classes.title}>
              {title}
              </Typography>
              <AccountMenu />
            </Toolbar>
          </AppBar> 
        </div>
      </React.Fragment>
    );
  }

export default AppHeader;
