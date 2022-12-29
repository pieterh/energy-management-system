import React from 'react';
import { styled } from '@mui/material/styles';
import { useTheme } from '@mui/material/styles';
import { useLocation } from 'react-router-dom';

import useMediaQuery from '@mui/material/useMediaQuery';

import AppBar from '@mui/material/AppBar';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';
import MenuOpenIcon from '@mui/icons-material/MenuOpen';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';

import { useAppSelector, useAppDispatch } from '../../common/hooks';

import { selectIsDrawerOpen, openDrawer, closeDrawer, toggleDrawer } from '../appdrawer/drawerSlice';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';
import AccountMenu from '../accountmenu/AccountMenu';

interface IStyleProps {
  isScreenXS: boolean
}

const PREFIX = 'DashboardCard';
const classes = {
  root: `${PREFIX}-root`,
  appBar: `${PREFIX}-appBar`,
  menuButton: `${PREFIX}-menuButton`,
  title: `${PREFIX}-title`,
  toolbar: `${PREFIX}-toolbar`,
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.root}`]: {
    display: 'flex',
  },
  [`& .${classes.appBar}`]: {

  },
  [`& .${classes.menuButton}`]: {
    marginRight: theme.spacing(2)
  },
  [`& .${classes.title}`]: {
    flexGrow: 1
  },
  [`& .${classes.toolbar}`]: {
    paddingLeft: 16,
  },     
}))

type Props = {
  children?: React.ReactNode;
};

export function AppHeader({children}: Props): JSX.Element {           
    const dispatch = useAppDispatch();
    const location = useLocation();  
    const theme = useTheme();

    const isLoggedIn = useAppSelector(selectIsLoggedIn);    
    const isScreenXS = useMediaQuery(theme.breakpoints.only('xs'));

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
