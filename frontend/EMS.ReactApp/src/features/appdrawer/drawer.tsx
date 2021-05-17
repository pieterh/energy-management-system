import React, { useState } from 'react';
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

interface IAppDrawerProps {
  persistent: boolean
}

const drawerContent = (
    <React.Fragment>       
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

export default function AppDrawer(props: IAppDrawerProps) {
  const dispatch = useAppDispatch();
  const classes = useStyles(); 
  const isDrawerOpen = useAppSelector(selectIsDrawerOpen);

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
    >
      {drawerContent}
    </Drawer>
  )
}
