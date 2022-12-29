import React, { useEffect } from 'react';
import { styled } from '@mui/material/styles';

import { useNavigate } from 'react-router-dom';
import clsx from 'clsx';

import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';

import EvStationIcon from '@mui/icons-material/EvStation';
import WbSunnyIcon from '@mui/icons-material/WbSunny';
import DashboardIcon from '@mui/icons-material/Dashboard';
import Drawer from '@mui/material/Drawer';

import { useAppDispatch, useAppSelector } from  '../../common/hooks';

import CarElectric from '../../icons/CarElectric';
import ElectricalServices from '../../icons/ElectricalServices';
import SettingsPowerIcon from '@mui/icons-material/SettingsPower';

import { selectIsDrawerOpen, openDrawer, closeDrawer, toggleDrawer } from './drawerSlice';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';

import { Page as EVSEPage } from '../chargepoint/Page';
import { Page as SmartMeterPage } from '../smartmeter/Page';
import { Page as EVPage } from '../ev/Page';
import { Page as HEMSPage } from '../hems/Page';

const drawerWidth = 240;

const PREFIX = 'drawerInfoWidget';
const classes = {
  root: `${PREFIX}-root`,
  paper: `${PREFIX}-paper`,
  drawer: `${PREFIX}-drawer`,
  drawerOpen: `${PREFIX}-drawerOpen`,
  drawerClose: `${PREFIX}-drawerClose`,    
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.root}`]: {
    flexGrow: 1,
  },
  [`& .${classes.paper}`]: {
    padding: theme.spacing(2),
    textAlign: "center",
    color: theme.palette.text.secondary,
  },
  [`& .${classes.drawer}`]: {
    marginBottom: 12,
  },
  [`& .${classes.drawerOpen}`]: {
    width: drawerWidth,
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  },  
  [`& .${classes.drawerClose}`]: {
    transition: theme.transitions.create('width', {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'hidden',
    width: theme.spacing(7) + 1,
  },  
}))

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
      id:"appdrawer-dashboard",
      key: "dashboard",
      icon: <DashboardIcon />,
      title: "Dashboard",
      route: "/",
      exactRoute: true,
      component: <> </>
    },     
    {
      id:"appdrawer-pvsystem",
      key: "pvsystem",
      icon: <WbSunnyIcon />,
      title: "Solar Power",
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
      component: <SmartMeterPage />
    },     
    {
      id:"appdrawer-evcar",
      key: "evcar",
      icon: <CarElectric />,
      title: "Electric Car",
      route: "/ev",
      exactRoute: false,
      component: <EVPage />
    },        
    {
      id:"appdrawer-hems",
      key: "hems",
      icon: <SettingsPowerIcon />,
      title: "HEMS",
      route: "/hems",
      exactRoute: false,
      component: <HEMSPage />
    },     
  ],
} 

interface IAppDrawerProps {
  persistent: boolean
}

export default function AppDrawer(props: IAppDrawerProps) {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
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
      navigate(item.route);
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
    <Root>
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
    </Root>
  )
}
