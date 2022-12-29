import React from 'react';
import { styled } from '@mui/material/styles';
import Dialog from '@mui/material/Dialog';
import CircularProgress from '@mui/material/CircularProgress';

const PREFIX = 'SplashScreenWidget';
const classes = {
  root: `${PREFIX}-root`,
  paper: `${PREFIX}-paper`,
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.root}`]: {
    flexGrow: 1,
  },
  [`& .${classes.paper}`]: {
    position: 'absolute',
    width: 100,
    height: 100,
    outline: `none`,
    border: 0,
    padding: theme.spacing(2, 4, 3),
    top: `${50}%`,
    left: `${50}%`,
    transform: `translate(-${50}%, -${50}%)`,
  },
}))

export type ISplashScreenProps = {
}


export default function SplashScreen(props : ISplashScreenProps) {
  return(
    <React.Fragment>
      <Root>
        <Dialog open={true}>        
          <div className={classes.paper}>
            <CircularProgress />
          </div>        
        </Dialog>
      </Root>
    </React.Fragment>
  )
};
