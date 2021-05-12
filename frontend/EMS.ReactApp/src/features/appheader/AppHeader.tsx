import React from 'react';
import clsx from 'clsx';

import { useLocation } from 'react-router-dom';
import useMediaQuery from '@material-ui/core/useMediaQuery';
import { makeStyles, Theme, useTheme } from '@material-ui/core/styles';
import { Icon } from "@material-ui/core";

import AppBar from '@material-ui/core/AppBar';
import Divider from '@material-ui/core/Divider';
import Drawer from '@material-ui/core/Drawer';
import IconButton from '@material-ui/core/IconButton';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';

import {green, yellow, red }from "@material-ui/core/colors";

import EvStationIcon from '@material-ui/icons/EvStation';
import MenuIcon from '@material-ui/icons/Menu';
import MenuOpenIcon from '@material-ui/icons/MenuOpen';
import SettingsIcon from '@material-ui/icons/Settings';
import WbSunnyIcon from '@material-ui/icons/WbSunny';
import DashboardIcon from '@material-ui/icons/Dashboard';

import { useAppSelector, useAppDispatch } from '../../common/hooks';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';
import AccountMenu from '../accountmenu/AccountMenu';

import CarElectric from '../../icons/CarElectric';
import ElectricalServices from '../../icons/ElectricalServices';

type FormInputs = {
  darkState : boolean
};

interface IAppHeaderProps {
}
  
interface IAppHeaderState {
}

interface IStyleProps {
  isScreenXS: boolean
}

const drawerWidth = 240;
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
  },
  hide: {
    display: 'none',
  },
  drawer: {
    flexShrink: 0,
    whiteSpace: 'nowrap',    
    position: 'fixed',
    zIndex:10,
  },
  drawerOpen: {
    width: drawerWidth,
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  },
  drawerClose: {
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'hidden',
    width: theme.spacing(7) + 1,
  },
  drawerHeader: {
    display: 'flex',
    alignItems: 'center',
    padding: theme.spacing(0, 1),
    // necessary for content to be below app bar
    ...theme.mixins.toolbar,
    justifyContent: 'flex-end',
  },
  toolbar: {
    paddingLeft: 16,
  },
  content: (p : IStyleProps) => ({ 
    flexGrow: 1,
    paddingLeft: theme.spacing(3) + 56, //(p.isScreenXS ? 0 : 56),
    paddingTop: theme.spacing(3),
    paddingRight: theme.spacing(3),
    paddingBottom: theme.spacing(3)    
  }),
}));

type Props = {
  children?: React.ReactNode;
};

export function AppHeader({children}: Props): JSX.Element{           
    const dispatch = useAppDispatch();
    const theme = useTheme();
    const isScreenXS = useMediaQuery(theme.breakpoints.only('xs'));
    console.log(`isSmallScreen? -> ${isScreenXS}`);
    console.log(`xs=${useMediaQuery(theme.breakpoints.only('xs'))} sm=${useMediaQuery(theme.breakpoints.only('sm'))} md=${useMediaQuery(theme.breakpoints.only('md'))} lg=${useMediaQuery(theme.breakpoints.only('lg'))}`  );

    const [isDrawerOpen, setDrawerOpen] = React.useState(false);

    const classes = useStyles({isScreenXS: isScreenXS}); 

    const isLoggedIn = useAppSelector(selectIsLoggedIn);
    const location = useLocation();  
    
    var title = "";
    title = location.pathname == '/' ? 'Main' : title;
    title = location.pathname == '/login' ? 'Login' : title;
    title = location.pathname == '/logout' ? 'Logout' : title;
    title = location.pathname == '/dashboard' ? 'Dashboard' : title;
    title = location.pathname == '/settings' ? 'Settings' : title;

    function onDrawerToggleClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent> | null)  {
      setDrawerOpen(!isDrawerOpen);
    }

    function onDrawerCloseClick(event: React.MouseEvent<HTMLButtonElement, MouseEvent> | null)  {
      setDrawerOpen(false);
    }

    const drawerContent = (
      <React.Fragment> 
        { <div className={classes.drawerHeader}/> }
        <List>
          <ListItem button key="pvsystem">
            <ListItemIcon> <WbSunnyIcon /> </ListItemIcon>
            <ListItemText primary="Solar Power" />
          </ListItem>
        </List>        
        <List>
          <ListItem button key="chargepoint">
            <ListItemIcon> <EvStationIcon /> </ListItemIcon>
            <ListItemText primary="chargepoint" />
          </ListItem>
        </List>    
        <List>
          <ListItem button key="smartmeter">
            <ListItemIcon> <ElectricalServices/> </ListItemIcon>
            <ListItemText primary="Smart Meter" />
          </ListItem>
        </List>      
        <List>
          <ListItem button key="evcar">
            <ListItemIcon> <CarElectric/> </ListItemIcon>
            <ListItemText primary="Electric Car" />
          </ListItem>
        </List>                  
        <List>
          <ListItem button key="dashboard">
            <ListItemIcon> <DashboardIcon/> </ListItemIcon>
            <ListItemText primary="Dashboard" />
          </ListItem>
        </List> 
        <Divider />
        <List>
          <ListItem button key="settings">
            <ListItemIcon> <SettingsIcon /> </ListItemIcon>
            <ListItemText primary="Settings" />
          </ListItem>
        </List> 
      </React.Fragment>
    );

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
        { isLoggedIn && isScreenXS &&
          <Drawer
            variant="persistent"
            className={clsx(classes.drawer, {
              [classes.drawerOpen]: isDrawerOpen,
              [classes.drawerClose]: !isDrawerOpen,
            })}
            classes={{
              paper: clsx({
                [classes.drawerOpen]: isDrawerOpen,
                [classes.drawerClose]: !isDrawerOpen,
              }),
            }}            
            anchor="left"
            open={isDrawerOpen}
            >
              {/* smUp */}
              {drawerContent}
          </Drawer>
        }
        { isLoggedIn && !isScreenXS &&          
          <Drawer
            variant="permanent"
            className={clsx(classes.drawer, {
              [classes.drawerOpen]: isDrawerOpen,
              [classes.drawerClose]: !isDrawerOpen,
            })}
            classes={{
              paper: clsx({
                [classes.drawerOpen]: isDrawerOpen,
                [classes.drawerClose]: !isDrawerOpen,
              }),
            }}                 
            open={isDrawerOpen}
          >
            {drawerContent}
          </Drawer>
        }</div>
        <main className={classes.content} >
          {children}
        </main>
      </React.Fragment>
    );
  }

export default AppHeader;

