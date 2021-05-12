import React, { useEffect } from 'react';
import { DateTime } from 'luxon';

import { makeStyles, createStyles, Theme } from '@material-ui/core/styles';

import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';

import { useAppSelector, useAppDispatch } from '../../common/hooks';
import { getSessionInfoAsync, selectSessionInfo } from '../chargepoint/EVSESlice';

import { DashboardCard } from '../../components/dashboardcard/DashboardCard';

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      flexGrow: 1,
    },
    paper: {
      padding: theme.spacing(2),
      textAlign: 'center',
      color: theme.palette.text.secondary,
    },
    pos: {
      marginBottom: 12,
    },
  }),
);

export function EVSEInfo() {
  const classes = useStyles();
  const dispatch = useAppDispatch();

  return(
    <React.Fragment>
      <DashboardCard title="EVSE Info">
        <Grid container item xs={12} spacing={1}>
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              availability
            </Typography>            
          </Grid>
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              mode 3 state
            </Typography> 
          </Grid>
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              current l1
            </Typography> 
          </Grid>
          <Grid item xs={4}>
          <Typography className={classes.pos} color="textSecondary">
              current l2
            </Typography> 
          </Grid>
          <Grid item xs={4}>
          <Typography className={classes.pos} color="textSecondary">
              current l3
            </Typography> 
          </Grid> 
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              voltage l1
            </Typography> 
          </Grid>
          <Grid item xs={4}>
          <Typography className={classes.pos} color="textSecondary">
              voltage l2
            </Typography> 
          </Grid>
          <Grid item xs={4}>
          <Typography className={classes.pos} color="textSecondary">
              voltage l3
            </Typography> 
          </Grid>                                  
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              delivered total
            </Typography>             
          </Grid>            
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              delivered l1
            </Typography>     
          </Grid>
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              delivered l2
            </Typography>     
          </Grid>
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              delivered l3
            </Typography>     
          </Grid>             
        </Grid>            
      </DashboardCard>
    </React.Fragment>
  )
};

export function EVSESessionInfo() {
  const classes = useStyles();
  const dispatch = useAppDispatch();
  const sessionInfo = useAppSelector(selectSessionInfo);  

  useEffect(() => {
    const interval = setInterval(() => {
      // hmmm object arg...
      dispatch(getSessionInfoAsync({id:1}));
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return(
    <React.Fragment>
      <DashboardCard title="Session info">
        <Grid container item xs={12} spacing={1}>
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              {sessionInfo?.mode3StateMessage}
            </Typography>                
          </Grid>
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              { sessionInfo?.lastChargingStateChangedFormatted }
            </Typography>   
          </Grid>            
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
            {sessionInfo?.phases}
            </Typography>
          </Grid>     
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              {sessionInfo?.maxCurrent} A
            </Typography>
          </Grid>   
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              {sessionInfo?.appliedMaxCurrent} A
            </Typography>
          </Grid>     
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              max power
            </Typography>
          </Grid>   
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              power
            </Typography>
          </Grid>                                           
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              voltage
            </Typography>
          </Grid> 
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              energy delivered
            </Typography>
          </Grid>  
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              time
            </Typography>
          </Grid>                                    
        </Grid>
      </DashboardCard>
    </React.Fragment>
  )
};
