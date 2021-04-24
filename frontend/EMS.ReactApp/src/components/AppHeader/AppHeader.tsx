
import React from 'react';
import { connect } from "react-redux"
import { Provider } from "react-redux";
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import IconButton from '@material-ui/core/IconButton';
import { default as MenuIcon } from '@material-ui/Icons/Menu';
import Typography from '@material-ui/core/Typography';
import Button from '@material-ui/core/Button';

import { createStyles, withStyles, Theme } from '@material-ui/core/styles';

import { store, RootState } from '../../App/store';
import { useDispatch, useSelector } from 'react-redux';

const styles = ({ palette, spacing }: Theme) => createStyles({
  });

interface IAppHeaderProps {
  classes: any
}
  
interface IAppHeaderState {
}
//const authentication = useSelector((state: RootState) => state.authentication);

class AppHeader extends React.Component<IAppHeaderProps, IAppHeaderState> {
    

    constructor(props: IAppHeaderProps) {
        super(props);  
        this.state = {
            materialUIclasses: {}
        }
    }
    render () {
        //
        return (
            <Provider store={store}>    
                <AppBar position="static">
                    <Toolbar>
                        <IconButton edge="start" className={this.props.classes.menuButton} color="inherit" aria-label="menu">
                        <MenuIcon />
                        </IconButton>
                        <Typography variant="h6" className={this.props.classes.title}>
                        News
                        </Typography>
                        {/* <div>
                        {authentication.state == 'logged_in'} && <Button color="inherit">Logout</Button>
                        </div>
                        <div>
                        {authentication.state == 'logged_out'} &&  <Button color="inherit">Login</Button>
                        </div> */}
                    </Toolbar>
                </AppBar> 
            </Provider>
        );
    }
}

export default withStyles(styles)(AppHeader)
