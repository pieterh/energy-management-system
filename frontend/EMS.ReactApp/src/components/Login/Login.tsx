import React from 'react';
import { compose, Dispatch } from 'redux';
import { connect, useDispatch } from "react-redux"
import { ThunkDispatch} from 'redux-thunk';
import { AnyAction } from '@reduxjs/toolkit';

import Avatar from '@material-ui/core/Avatar';
import Box from '@material-ui/core/Box';
import Button from '@material-ui/core/Button';
import Checkbox from '@material-ui/core/Checkbox';
import Container from '@material-ui/core/Container';
import CssBaseline from '@material-ui/core/CssBaseline';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Grid from '@material-ui/core/Grid';
import Link from '@material-ui/core/Link';
import TextField from '@material-ui/core/TextField';
import Typography from '@material-ui/core/Typography';

import { createStyles, withStyles, Theme } from '@material-ui/core/styles';
import LockOutlinedIcon from '@material-ui/icons/LockOutlined';

import { RootState } from '../../App/store';
import { loginAsync, logoutAsync, increment } from '../../App/authenticationSlice';

import  Credits from '../Credits/Credits';

import './Login.css';

const styles = ({ palette, spacing }: Theme) => createStyles({
  paper: {
    marginTop: spacing(8),
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
  },
  avatar: {
    margin: spacing(1),
    backgroundColor: palette.secondary.main,
  },
  form: {
    width: '100%', // Fix IE 11 issue.
    marginTop: spacing(1),
  },
  submit: {
    margin: spacing(3, 0, 2),
  },
});

interface ILoginProps {
  classes: any,
}

interface ILoginState {
  username: string,
  password: string
}

interface IPropsFromDispatch {
  login: typeof loginAsync,
  logout: typeof logoutAsync,
  increment: typeof increment,
  dispatch: Dispatch,
  thunkDispatch: ThunkDispatch<{}, any, AnyAction>
}

// Combine both state + dispatch props - as well as any props we want to pass - in a union type.
type AllProps = ILoginProps & ILoginState & IPropsFromDispatch;// & RouteComponentProps<RouteParams>

class Login extends React.Component<AllProps, ILoginState> {
  state: ILoginState;

  constructor(props: AllProps, state: ILoginState) {
      super(props);
      this.state = state;

      this.state = {
           username : '',
           password: ''
       }
  }

  increment = (event: React.MouseEvent<HTMLButtonElement>) => {
    console.log("click increment");
    this.props.dispatch(this.props.increment());
  }

  loginClick = async (event: React.MouseEvent<HTMLButtonElement>) => {
    console.log("click ->");
    await this.props.thunkDispatch(loginAsync({username: this.state.username, secret: this.state.password})).then((x) =>
    {
      console.log("then " + x.payload);
    });    
    console.log("click <-");
  }

  handleSubmit(event: React.FormEvent<HTMLFormElement>){
    console.log("form");
    event.preventDefault();
  }

  handleUserNameFieldChange = (event: React.ChangeEvent<HTMLInputElement>) =>{
    this.setState({ username: event.target.value});
  }

  handlePasswordFieldChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    this.setState({ password: event.target.value});
  }

  render() {
    return(
      <Container component="main" maxWidth="xs">
        <CssBaseline />
        <div className={this.props.classes.paper}>
          <Avatar className={this.props.classes.avatar}>
            <LockOutlinedIcon />
          </Avatar>
          <Typography component="h1" variant="h5">
            Sign in
          </Typography>
          <form className={this.props.classes.form} noValidate onSubmit={this.handleSubmit}>
            <TextField
              variant="outlined"
              margin="normal"
              required
              fullWidth
              id="username"
              label="Username"
              name="username"
              autoComplete="username"
              autoFocus
              value={this.state.username}
              onChange={this.handleUserNameFieldChange}
            />
            <TextField
              variant="outlined"
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete="current-password"
              value={this.state.password}
              onChange={this.handlePasswordFieldChange}
            />
            <FormControlLabel
              control={<Checkbox value="remember" color="primary" />}
              label="Remember me"
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              color="primary"
              className={this.props.classes.submit}
              onClick={this.loginClick}
            >
              Sign In
            </Button>
            <Grid container>
              <Grid item xs>
                <Link href="#" variant="body2">
                  Forgot password?
                </Link>
              </Grid>
            </Grid>
          </form>
        </div>
        <Box mt={8}>
          <Credits />
        </Box>
      </Container>
    )
  }
}

function mapStateToProps (state: RootState, ownProps: {}) {
  return {
    authentication: state.authentication
  };
};

function mapDispatchToProps (dispatch: Dispatch) {
  return {
    login : loginAsync,
    logout: logoutAsync,
    increment: increment,
    dispatch: dispatch,
    thunkDispatch: dispatch as ThunkDispatch<{}, any, AnyAction>
  }
}

export default compose(
    connect(mapStateToProps, mapDispatchToProps),
    withStyles(styles)
  )(Login);
