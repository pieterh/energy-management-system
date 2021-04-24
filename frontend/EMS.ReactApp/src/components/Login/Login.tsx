import React, { useState } from 'react';

import Avatar from '@material-ui/core/Avatar';
import Box from '@material-ui/core/Box';
import Button from '@material-ui/core/Button';
import Checkbox from '@material-ui/core/Checkbox';
import Container from '@material-ui/core/Container';
import CssBaseline from '@material-ui/core/CssBaseline';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import TextField from '@material-ui/core/TextField';
import Typography from '@material-ui/core/Typography';
import CircularProgress from '@material-ui/core/CircularProgress';

import { createStyles, makeStyles, Theme } from '@material-ui/core/styles';
import LockOutlinedIcon from '@material-ui/icons/LockOutlined';

import { RootState, useAppDispatch } from '../../App/store';
import { loginAsync, logoutAsync, increment } from '../../App/authenticationSlice';

import  Credits from '../Credits/Credits';

import './Login.css';

const useStyles = makeStyles ((theme: Theme) => 
  createStyles({
    paper: {
      marginTop: theme.spacing(8),
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
    },
    avatar: {
      margin: theme.spacing(1),
      backgroundColor: theme.palette.secondary.main,
    },
    form: {
      width: '100%', // Fix IE 11 issue.
      marginTop: theme.spacing(1),
    },
    submit: {
      margin: theme.spacing(3, 0, 2),
    },
    spinner:{
      width: 20,
      height: 20,
      size: 20,
    }
  })
);

interface ILoginProps {
  //
}
interface ILoginTheme {
  classes: any,
}

interface ILoginState {
}

export function Login() {
  const dispatch = useAppDispatch()
  const classes = useStyles(); 

  const [isBusy, setIsBusy] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>){
    console.log("form ->");
    event.preventDefault();
    setIsBusy(true);    

    dispatch(loginAsync({username: username, secret: password})).then((x) =>{
      console.log("form <-");
    }).finally(() =>{
      setIsBusy(false); 
      console.log("form <- finally");
    });  
  }

  return (
    <Container component="main" maxWidth="xs">
      <CssBaseline />
      <div className={classes.paper}>
        <Avatar className={classes.avatar}>
          <LockOutlinedIcon />
        </Avatar>
        <Typography component="h1" variant="h5">
          Sign in
        </Typography>
        <form className={classes.form} noValidate onSubmit={handleSubmit}>
          <TextField
            disabled={isBusy}
            variant="outlined"
            margin="normal"
            required
            fullWidth
            id="username"
            label="Username"
            name="username"
            autoComplete="username"
            autoFocus
            value={username}
            onChange={(event: React.ChangeEvent<HTMLInputElement>) => setUsername(event.target.value)}
          />
          <TextField
            disabled={isBusy}
            variant="outlined"
            margin="normal"
            required
            fullWidth
            name="password"
            label="Password"
            type="password"
            id="password"
            autoComplete="current-password"
            value={password}
            onChange={(event: React.ChangeEvent<HTMLInputElement>) => setPassword(event.target.value)}      
          />
          <FormControlLabel
            disabled={isBusy}
            control={<Checkbox value="remember" color="primary" />}
            label="Remember me"            
          />
          <Button
            disabled={isBusy}
            type="submit"
            fullWidth
            variant="contained"
            color="primary"
            className={classes.submit}
          >
            Sign In
            {/* className={classes.spinner} */}
            { isBusy && <CircularProgress size={20} /> }
          </Button>          
        </form>
      </div>
      <Box mt={8}>
        <Credits />
      </Box>
    </Container>
  )
}

 export default Login;
 //export default withStyles(styles)(Login);

// function mapStateToProps (state: RootState, ownProps: {}) {
//   return {
//     authentication: state.authentication
//   };
// };

// function mapDispatchToProps (dispatch: Dispatch) {
//   return {
//     login : loginAsync,
//     logout: logoutAsync,
//     increment: increment,
//     dispatch: dispatch,
//     thunkDispatch: dispatch as ThunkDispatch<{}, any, AnyAction>
//   }
// }

// export default compose(
//     connect(mapStateToProps, mapDispatchToProps),
//     withStyles(styles)
//   )(Login);
