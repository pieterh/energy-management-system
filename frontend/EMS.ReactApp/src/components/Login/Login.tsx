import React, { useState } from 'react';
import { Redirect } from 'react-router-dom';

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

import { useForm, Controller } from "react-hook-form";

import { loginAsync, isLoggedIn } from '../../App/authenticationSlice';

import { useAppSelector, useAppDispatch } from '../../App/hooks';

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

type FormInputs = {
  username: string,
  password: string,
  doRemember: string
};

export function Login() {
  const loggedIn = useAppSelector(isLoggedIn);
  const dispatch = useAppDispatch();
  const classes = useStyles(); 
  
  const { register, handleSubmit, watch, control, reset, formState: { errors } } = useForm<FormInputs>();

  const [isBusy, setIsBusy] = useState(false);

  async function onSubmit(data : FormInputs){
    console.log(`form -> ${JSON.stringify(data)}`);
    setIsBusy(true);    

    dispatch(loginAsync({username: data.username, secret: data.password})).then((x) =>{
      console.log("form <-");
    }).finally(() =>{
      setIsBusy(false); 
      console.log("form <- finally");
    });  
  }
  if (loggedIn) { return (<Redirect to='/'/>); } else
  return (
    <Container component="main" maxWidth="xs">     
      <div className={classes.paper}>
        <Avatar className={classes.avatar}>
          <LockOutlinedIcon />
        </Avatar>
        <Typography component="h1" variant="h5">
          Sign in
        </Typography>
        <form className={classes.form} onSubmit={handleSubmit(onSubmit)}>
          <TextField
            disabled={isBusy}
            variant="outlined"
            margin="normal"
            required
            fullWidth
            id="username"
            label="Username"
            autoComplete="username"
            autoFocus
            {...register("username", { required: true })}
          />
          {errors.username && <span>This username field is required</span>}
          <TextField
            disabled={isBusy}
            variant="outlined"
            margin="normal"
            required
            fullWidth
            label="Password"
            type="password"
            id="password"
            autoComplete="current-password"
            {...register("password", { required: true })}  
          />
          {errors.password && <span>This password field is required</span>}
          <Controller
            name="doRemember"
            control={control}
            defaultValue={false}
            rules={{ required: false }}
            render={({ field }) => {
              return <FormControlLabel
                        disabled={isBusy}
                        control={<Checkbox  color="primary" {...field}/>}
                        label="Remember me"
                    />
            }}            
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

