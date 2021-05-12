import React from 'react';
import { useHistory } from 'react-router-dom';

import ClickAwayListener from '@material-ui/core/ClickAwayListener';
import Divider from '@material-ui/core/Divider';
import Grow from '@material-ui/core/Grow';
import MenuItem from '@material-ui/core/MenuItem';
import MenuList from '@material-ui/core/MenuList';
import MuiIconButton from '@material-ui/core/IconButton';
import Paper from '@material-ui/core/Paper';
import Popper from '@material-ui/core/Popper';

import AccountBoxIcon from '@material-ui/icons/AccountBox';
import ArrowBackIcon from '@material-ui/icons/ArrowBack';
import Brightness4Icon from '@material-ui/icons/Brightness4';
import DesktopWindowsIcon from '@material-ui/icons/DesktopWindows';
import ExitToAppIcon from '@material-ui/icons/ExitToApp';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import NightsStayIcon from '@material-ui/icons/NightsStay';
import PersonIcon from '@material-ui/icons/Person';
import SettingsIcon from '@material-ui/icons/Settings';
import WbSunnyIcon from '@material-ui/icons/WbSunny';

import { useAppSelector, useAppDispatch } from '../../App/hooks';
import { ChangeTheme, ThemeTypes } from '../themeprovider/CustomThemeProviderSlice';

interface AccountMenuProps {
  onMenuOpen?: () => void;
}

export function AccountMenu(props: AccountMenuProps): JSX.Element{ 
    const dispatch = useAppDispatch();
    const history = useHistory();

    const isLoggedIn = useAppSelector( state => state.authentication.isLoggedIn ) as boolean;
    const themeName = useAppSelector( state => state.customTheme.themeName ) as string;

    const showLoginMenuItem = !isLoggedIn && location.pathname != '/login' && location.pathname != '/logout';

    const buttonRef = React.useRef(null);

    const handleThemeChange = (theme:ThemeTypes) => {
        dispatch(ChangeTheme(theme));
        setAnchorEl(null); // close main menu
      };

    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);

    const handleOpenMain = () => {
        setTopBarState(topbarstate.Main);
      }
    const handleOpenAppearance = () => {
      setTopBarState(topbarstate.Appearance);
    };

    const handleToggle = () => {
      handleOpenMain();
      setAnchorEl(buttonRef.current);
      // setDrawerOpen(false);
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
  
    function onLoginClick(event: React.MouseEvent<HTMLLIElement, MouseEvent> | null)  {
      setAnchorEl(null);
      history.push('/login');
    }

    function onLogoutClick(event: React.MouseEvent<HTMLLIElement, MouseEvent> | null)  {
      setAnchorEl(null);
      history.push('/logout');
    }

    enum topbarstate {Main, Appearance}
    const [topBarState, setTopBarState] = React.useState(topbarstate.Main);
    const createTopBarMenuF = () => {
      if (showLoginMenuItem) 
        return [<MenuItem key="a" onClick={onLoginClick}>Login</MenuItem>];

        if (topBarState == topbarstate.Main)
          return [                  
            <MenuItem key="a" onClick={handleOpenAppearance}><ListItemIcon><Brightness4Icon/></ListItemIcon>Appearance: {themeName}</MenuItem>,
            <MenuItem key="b" onClick={handleClose}><ListItemIcon><AccountBoxIcon/></ListItemIcon>My account</MenuItem>,
            <MenuItem key="c" onClick={handleClose}><ListItemIcon><SettingsIcon/></ListItemIcon>Settings</MenuItem>,
            <Divider key="d" />,
            <MenuItem key="e" onClick={onLogoutClick}><ListItemIcon><ExitToAppIcon/></ListItemIcon>Logout</MenuItem>,
          ];
          if (topBarState == topbarstate.Appearance) {
            return [
              <MenuItem key="a" onClick={handleOpenMain}><ListItemIcon><ArrowBackIcon/></ListItemIcon>Appearance</MenuItem>,
              <Divider key="b" />,
              <div key="c" >Settings applies to this browser only</div>,
              <MenuItem key="d" onClick={() => handleThemeChange(ThemeTypes.device)}><ListItemIcon><DesktopWindowsIcon/></ListItemIcon>Use device theme</MenuItem>,
              <MenuItem key="e" onClick={() => handleThemeChange(ThemeTypes.dark)}><ListItemIcon><NightsStayIcon/></ListItemIcon>Dark theme</MenuItem>,
              <MenuItem key="bf" onClick={() => handleThemeChange(ThemeTypes.light)}><ListItemIcon><WbSunnyIcon/></ListItemIcon>Light theme</MenuItem>,
            ];              
          }                
        return [];
    };

    const createTopBarMenu = createTopBarMenuF();  

    return (
        <React.Fragment>
          <MuiIconButton ref={buttonRef} aria-controls={open ? 'menu-list-grow' : undefined} aria-haspopup="true" onClick={handleToggle}> 
              <PersonIcon/> 
          </MuiIconButton>       
          <Popper open={open} anchorEl={anchorEl} role={undefined} transition disablePortal>
            {({ TransitionProps, placement }) => (
              <Grow
              {...TransitionProps}
              style={{ transformOrigin: placement === 'bottom' ? 'center top' : 'center bottom' }}
              >
                <Paper>
                    <ClickAwayListener onClickAway={handleClose}>
                      <MenuList autoFocusItem={open} id="menu-list-grow" onKeyDown={handleListKeyDown}>                             
                        {createTopBarMenu}  
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
