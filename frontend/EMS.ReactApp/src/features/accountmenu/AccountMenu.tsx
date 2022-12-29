import React,{ useMemo } from 'react';
import { useNavigate } from 'react-router-dom';

import ClickAwayListener from '@mui/material/ClickAwayListener';
import Divider from '@mui/material/Divider';
import Grow from '@mui/material/Grow';
import MenuItem from '@mui/material/MenuItem';
import MenuList from '@mui/material/MenuList';
import MuiIconButton from '@mui/material/IconButton';
import Paper from '@mui/material/Paper';
import Popper from '@mui/material/Popper';

import AccountBoxIcon from '@mui/icons-material/AccountBox';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import DesktopWindowsIcon from '@mui/icons-material/DesktopWindows';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import ListItemIcon from '@mui/material/ListItemIcon';
import NightsStayIcon from '@mui/icons-material/NightsStay';
import PersonIcon from '@mui/icons-material/Person';
import SettingsIcon from '@mui/icons-material/Settings';
import WbSunnyIcon from '@mui/icons-material/WbSunny';

import { useAppSelector, useAppDispatch } from  '../../common/hooks';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';

import { ChangeTheme, ThemeTypes } from '../themeprovider/CustomThemeProviderSlice';

interface AccountMenuProps {
  onMenuOpen?: () => void;
}

export function AccountMenu(props: AccountMenuProps): JSX.Element{ 
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    const isLoggedIn = useAppSelector(selectIsLoggedIn);
    const themeName = useAppSelector( state => state.customTheme.themeName ) as string;

    const showLoginMenuItem = !isLoggedIn && location.pathname != '/login' && location.pathname != '/logout';

    const buttonRef = React.useRef(null);
    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);

    const handleOpenMain = () => {
        setTopBarState(topbarstate.Main);
      }

    const handleToggle = () => {
      handleOpenMain();
      setAnchorEl(buttonRef.current);
    };
    
    const handleClose = (/*event: React.MouseEvent<{}>*/) => {
      setAnchorEl(null);
    };
  
    function handleListKeyDown(event: React.KeyboardEvent<HTMLUListElement>) {
      if (event.key === 'Tab') {
        event.preventDefault();
        setAnchorEl(null);
      }
    }
  
    function onMenuClick(event: React.MouseEvent<HTMLLIElement, MouseEvent> | null)  {
      switch(event?.currentTarget.id) {
        case "menu-back":
          handleOpenMain();
          break;
        case "menu-theme":
          setTopBarState(topbarstate.Appearance);
          break;
        case "menu-login":
          navigate('/login');
          setAnchorEl(null);
          break;          
        case "menu-logout":
          navigate('/logout');
          setAnchorEl(null);
          break;
        case "menu-settings":
          navigate('/settings');
          setAnchorEl(null);
          break;
        case "menu-theme-device":
          dispatch(ChangeTheme(ThemeTypes.device));
          setAnchorEl(null);
          break;          
        case "menu-theme-dark":
          dispatch(ChangeTheme(ThemeTypes.dark));
          setAnchorEl(null);
          break;
        case "menu-theme-light":
          dispatch(ChangeTheme(ThemeTypes.light));
          setAnchorEl(null);
          break;
      }      
    }

    enum topbarstate {Main, Appearance}
    const [topBarState, setTopBarState] = React.useState(topbarstate.Main);
    const topBarMenuItems = useMemo(() => {
      if (showLoginMenuItem) 
        return [ <MenuItem id="menu-login" key="login" onClick={onMenuClick}>Login</MenuItem> ];

        if (topBarState == topbarstate.Main)
          return [                  
            <MenuItem id="menu-theme" key="theme" onClick={onMenuClick}><ListItemIcon><Brightness4Icon/></ListItemIcon>Appearance: {themeName}</MenuItem>,
            <MenuItem id="menu-myaccount" key="myaccount" onClick={onMenuClick}><ListItemIcon><AccountBoxIcon/></ListItemIcon>My account</MenuItem>,
            <MenuItem id="menu-settings" key="settings" onClick={onMenuClick}><ListItemIcon><SettingsIcon/></ListItemIcon>Settings</MenuItem>,
            <Divider key="divider-1" />,
            <MenuItem id="menu-logout" key="logout" onClick={onMenuClick}><ListItemIcon><ExitToAppIcon/></ListItemIcon>Logout</MenuItem>,
          ];
        if (topBarState == topbarstate.Appearance) 
          return [
            <MenuItem key="menu-back" onClick={onMenuClick}><ListItemIcon><ArrowBackIcon/></ListItemIcon>Appearance</MenuItem>,
            <Divider key="divider-1" />,
            <div key="div-1" >Settings applies to this browser only</div>,
            <MenuItem id="menu-theme-device" key="device" onClick={onMenuClick}><ListItemIcon><DesktopWindowsIcon/></ListItemIcon>Use device theme</MenuItem>,
            <MenuItem id="menu-theme-dark" key="dark" onClick={onMenuClick}><ListItemIcon><NightsStayIcon/></ListItemIcon>Dark theme</MenuItem>,
            <MenuItem id="menu-theme-light" key="light" onClick={onMenuClick}><ListItemIcon><WbSunnyIcon/></ListItemIcon>Light theme</MenuItem>,
          ]; 
        return [];
    }, [showLoginMenuItem, topBarState, themeName]);


    return (
        <React.Fragment>
          <MuiIconButton aria-label="account menu" ref={buttonRef} aria-controls={open ? 'menu-list-grow' : undefined} aria-haspopup="true" onClick={handleToggle}> 
              <PersonIcon/> 
          </MuiIconButton>       
          <Popper open={open} anchorEl={anchorEl} role={undefined} transition disablePortal onResize={undefined} onResizeCapture={undefined}>
            {({ TransitionProps, placement }) => (
              <Grow
              {...TransitionProps}
              style={{ transformOrigin: placement === 'bottom' ? 'center top' : 'center bottom' }}
              >
                <Paper>
                    <ClickAwayListener onClickAway={handleClose}>
                      <MenuList autoFocusItem={open} id="menu-list-grow" onKeyDown={handleListKeyDown}>                             
                        {topBarMenuItems}  
                      </MenuList>
                    </ClickAwayListener>
                </Paper>
              </Grow>
            )}
          </Popper>
        </React.Fragment>
    );
};

export default AccountMenu;
