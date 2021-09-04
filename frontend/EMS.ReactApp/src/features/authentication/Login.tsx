import React, { useState } from 'react';
import { Redirect } from 'react-router-dom';
import  browserStorage  from 'store2';

import { useForm, Controller, FormProvider } from "react-hook-form";
import { DevTool } from "@hookform/devtools";

import Avatar from '@material-ui/core/Avatar';
import Box from '@material-ui/core/Box';
import Button from '@material-ui/core/Button';
import Checkbox from '@material-ui/core/Checkbox';
import Container from '@material-ui/core/Container';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import TextField from '@material-ui/core/TextField';
import Typography from '@material-ui/core/Typography';
import CircularProgress from '@material-ui/core/CircularProgress';
import { createStyles, makeStyles, Theme } from '@material-ui/core/styles';
import LockOutlinedIcon from '@material-ui/icons/LockOutlined';
import { Alert } from '@material-ui/lab/';


import { useAppSelector, useAppDispatch } from  '../../common/hooks';
import { loginAsync, selectIsLoggedIn } from './authenticationSlice';
import  Credits from '../credits/Credits';
 
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

type FormInputs = {
  username: string,
  password: string,
  doRemember: boolean
};

export function Login() {
  const isLoggedIn = useAppSelector(selectIsLoggedIn);
  const dispatch = useAppDispatch();
  const classes = useStyles(); 
  
  const doRemember = browserStorage.local.has("rememberme");
  const username = doRemember && browserStorage.local.has("username") ? browserStorage.local.get("username") : "";
  
  var formRegistration = useForm<FormInputs>( {
    defaultValues: {
      username: username,
      password: "",
      doRemember: doRemember
     }
  });

  const [isBusy, setIsBusy] = useState(false);
  const [hasError, setHasError] = useState(false);

  async function onSubmit(data : FormInputs){
    setHasError(false);
    setIsBusy(true);    

    dispatch(loginAsync({username: data.username, secret: data.password, doRemember: data.doRemember})).then((x) => {
      switch(x.payload?.status) {
        case 200:          
          break;
        default:          
          console.error(x.payload?.message);
          setIsBusy(false);
          setHasError(true);          
          break;
      }      
    }).catch((e) => {
      console.log(e);
      setIsBusy(false);
      setHasError(true);
    });  
  }

  if (isLoggedIn) { return (<Redirect to='/'/>); } else
  return (
    <Container maxWidth="xs">     
      <div className={classes.paper}>
        <Avatar className={classes.avatar}>
          <LockOutlinedIcon />
        </Avatar>
        <Typography component="h1" variant="h5">
          Sign in
        </Typography>
        <FormProvider {...formRegistration}>
          {/* {process.env.NODE_ENV === 'development' &&  
          <DevTool control={formRegistration.control} placement="top-right"/> } */}
          
          <form className={classes.form} onSubmit={formRegistration.handleSubmit(onSubmit)}>
            <Controller
              name="username"            
              render={({ field: { value, onChange } }) => (
                <TextField id="username" disabled={isBusy} variant="outlined" margin="normal" required fullWidth  
                  label="Username" autoComplete="username" autoFocus value={value} onChange={onChange}                 
                />
              )}
            />            
            {formRegistration.formState.errors.username && <span>This username field is required</span>}

            <Controller
              name="password"            
              render={({ field: { value, onChange } }) => (
                <TextField id="password" type="password" disabled={isBusy} variant="outlined" margin="normal" required fullWidth  
                  label="Password" autoComplete="current-password" autoFocus value={value} onChange={onChange}                 
                />
              )}
            />  
            {formRegistration.formState.errors.password && <span>This password field is required</span>}

            <Controller
              name="doRemember"
              render={({ field: { value, onChange } }) => (
                // Checkbox accepts its value as `checked`
                // so we need to connect the props here 
                <FormControlLabel
                  control={<Checkbox id="doremember" checked={value} onChange={onChange} disabled={isBusy}/>}
                  label="Remember me"
                />
              )}
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
            {hasError &&
              <Alert severity="warning" >Unknown username or password</Alert>            
            }
          </form>
        </FormProvider>
      </div>
      <Box mt={8}>
        <Credits />
      </Box>
    </Container>    
  )
}

export default Login;
