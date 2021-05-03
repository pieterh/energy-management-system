import React from 'react';
import Modal from '@material-ui/core/Modal';
import CircularProgress from '@material-ui/core/CircularProgress';

import { makeStyles, createStyles, Theme } from '@material-ui/core/styles';

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    paper: {
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
  })
);

export type ISplashScreenProps = {
}

export default function SplashScreen(props : ISplashScreenProps) {
  const classes = useStyles();
  
  return(
    <React.Fragment>
        <Modal open={true}>
          <div className={classes.paper}>
            <CircularProgress />
          </div>
        </Modal>
    </React.Fragment>
  )
};
