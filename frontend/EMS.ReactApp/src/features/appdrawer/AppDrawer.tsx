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

import { useAppDispatch } from  '../../common/hooks';

import CarElectric from '../../icons/CarElectric';
import ElectricalServices from '../../icons/ElectricalServices';

const drawerWidth = 240;
const useStyles = makeStyles ((theme: Theme) => 
  createStyles({
    drawerHeader: {
        display: 'flex',
        alignItems: 'center',
        padding: theme.spacing(0, 1),
        // necessary for content to be below app bar
        ...theme.mixins.toolbar,
        justifyContent: 'flex-end',
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
  })
);

interface IAppDrawerProps {
  persistent: boolean,
  drawerOpen: boolean
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

  return (
    <Drawer
        variant= {props.persistent ? "persistent" : "permanent"}
        className={clsx(classes.drawer, {
        [classes.drawerOpen]: props.drawerOpen,
        [classes.drawerClose]: !props.drawerOpen,
        })}
        classes={{
        paper: clsx({
            [classes.drawerOpen]: props.drawerOpen,
            [classes.drawerClose]: !props.drawerOpen,
        }),
        }}            
        anchor="left"
        open={props.drawerOpen}
    >
      <div className={classes.drawerHeader}/>
      {drawerContent}
    </Drawer>
  )
}
