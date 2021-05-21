import React, { useState } from 'react';
import { useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import clsx from 'clsx';

import Divider from '@material-ui/core/Divider';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import { createStyles, makeStyles, Theme } from '@material-ui/core/styles';

import EvStationIcon from '@material-ui/icons/EvStation';
import SettingsIcon from '@material-ui/icons/Settings';
import WbSunnyIcon from '@material-ui/icons/WbSunny';
import DashboardIcon from '@material-ui/icons/Dashboard';
import Drawer from '@material-ui/core/Drawer';

import { useAppDispatch, useAppSelector } from  '../../common/hooks';

import CarElectric from '../../icons/CarElectric';
import ElectricalServices from '../../icons/ElectricalServices';
import { selectIsDrawerOpen, openDrawer, closeDrawer, toggleDrawer } from './drawerSlice';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';

import { Page as EVSEPage } from '../chargepoint/Page';

const drawerWidth = 240;
const useStyles = makeStyles ((theme: Theme) => 
  createStyles({   
    drawer: {
        flexShrink: 0,
        whiteSpace: 'nowrap',    
        position: 'fixed',
        zIndex:10,
      },
    paper: {
        top: 'unset',
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
  })
);

export type DrawerItem = {
  id: string,
  key: string,
  icon: React.ReactNode,
  title: string,
  route: string 
  exactRoute: boolean,
  component: React.ReactNode,
}

export type DrawerDefinitionT = {
  items: DrawerItem[]
}

export const DrawerDefinition : DrawerDefinitionT = {
  items: [
    {
      id:"appdrawer-pvsystem",
      key: "pvsystem",
      icon: <WbSunnyIcon />,
      title: "Solar Power!",
      route: "/pvsystem",
      exactRoute: false,
      component: <WbSunnyIcon />
    },
    {
      id:"appdrawer-evse",
      key: "evse",
      icon: <EvStationIcon />,
      title: "Charge Point",
      route: "/evse",
      exactRoute: false,
      component: <EVSEPage />
    },    
    {
      id:"appdrawer-smartmeter",
      key: "smartmeter",
      icon: <ElectricalServices />,
      title: "Smart meter",
      route: "/smartmeter",
      exactRoute: false,
      component: <ElectricalServices />
    },     
    {
      id:"appdrawer-evcar",
      key: "evcar",
      icon: <CarElectric />,
      title: "Electric Car",
      route: "/ev",
      exactRoute: false,
      component: <CarElectric />
    },         
    {
      id:"appdrawer-dashboard",
      key: "dashboard",
      icon: <DashboardIcon />,
      title: "Dashboard",
      route: "/",
      exactRoute: true,
      component: <> </>
    },      
  ],
} 

interface IAppDrawerProps {
  persistent: boolean
}

export default function AppDrawer(props: IAppDrawerProps) {
  const history = useHistory();
  const dispatch = useAppDispatch();
  const classes = useStyles(); 
  const isLoggedIn = useAppSelector(selectIsLoggedIn);    
  const isDrawerOpen = useAppSelector(selectIsDrawerOpen);

  const drawerContent = DrawerDefinition.items.map((item, i) => (
    <List key={item.key}>
      <ListItem id={"appdrawer-"+item.key} button onClick={onDrawerItemClick} >
        <ListItemIcon> {item.icon} </ListItemIcon>
        <ListItemText primary={item.title} />
      </ListItem>
    </List>  
  ));

  function onDrawerItemClick(event: React.MouseEvent<HTMLDivElement> | null) {
    var item = DrawerDefinition.items.find((x) => { return  x.id === event?.currentTarget?.id });
    if (!!item){
      history.push(item.route);
    } else {
      console.error("click item nog found ->" + event?.currentTarget?.id);
    }
  }

  // just close the drawer when we are logged out
  useEffect(() => {  
    if (!isLoggedIn && isDrawerOpen){
      dispatch(closeDrawer());
    }
  }, [isLoggedIn, isDrawerOpen]);

  return (
    <Drawer
        variant= {props.persistent ? "persistent" : "permanent"}
        className={clsx(classes.drawer, {
        [classes.drawerOpen]: isDrawerOpen,
        [classes.drawerClose]: !isDrawerOpen,
        })}
        classes={{
        paper: clsx(classes.paper,{
            [classes.drawerOpen]: isDrawerOpen,
            [classes.drawerClose]: !isDrawerOpen,
        }),
        }}            
        anchor="left"
        open={isDrawerOpen}
        hidden={!isLoggedIn}
    >
      <React.Fragment>       
        { drawerContent }  
      </React.Fragment>
    </Drawer>
  )
}
