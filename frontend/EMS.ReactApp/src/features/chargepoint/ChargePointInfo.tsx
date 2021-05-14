import React, { useEffect } from 'react';

import { makeStyles, createStyles, Theme } from '@material-ui/core/styles';
import Avatar from '@material-ui/core/Avatar';
import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';
import EvStationIcon from '@material-ui/icons/EvStation';

import { useAppSelector, useAppDispatch } from '../../common/hooks';
import { getSessionInfoAsync, selectSessionInfo, selectSocketInfo } from '../chargepoint/EVSESlice';

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

  // className={classes.avatar}
  return(
    <React.Fragment>
      <DashboardCard 
        title="Alfen Eve Single Pro-line" 
        subheader="ALF-0000307"
        avatar={
          <Avatar>
            <EvStationIcon />
          </Avatar>
        }
      >
        <Grid container item xs={12} spacing={1}>
          <Grid item xs={12}>
            <Typography className={classes.pos}  variant="body2" component="p">
              Backoffice is connected<br/>
              Firmware version 4.14.0-3398<br/>
              Uptime 14days
            </Typography>            
          </Grid>               
        </Grid>            
      </DashboardCard>
    </React.Fragment>
  )
};

export interface ISocketInfoProps {
  socketnr: number;
}
export function EVSESocketInfo(props: ISocketInfoProps) {
  const classes = useStyles();
  const sessionInfo = useAppSelector(selectSessionInfo);  
  const socketInfo = useAppSelector(selectSocketInfo);    

  return(
    <React.Fragment>
      <DashboardCard 
        title="Alfen Eve Single Pro-line"
        subheader={'ALF-0000307 Socket #' + props.socketnr }
        avatar={
          <Avatar>
            <EvStationIcon />
          </Avatar>
        }
      >
        <Grid container item xs={12} spacing={1}>
          <Grid item xs={12}>
            <Typography className={classes.pos}  variant="body2" component="p">
              { socketInfo?.availability ? "Available" : "Unavailable"}<br/>
              { socketInfo?.mode3StateMessage }<br/>
              { socketInfo?.realEnergyDelivered } kWh energy delivered<br/>             
              { socketInfo?.powerAvailable } <br/>
              { socketInfo?.vehicleIsConnected && <> { socketInfo?.powerUsing }                            </> }
            </Typography>            
          </Grid>

          {/* <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.voltage} V
            </Typography> 
          </Grid> */}




          {/* <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
            {socketInfo?.current} A
            </Typography> 
          </Grid>

          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">              
            </Typography> 
          </Grid> 
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.vehicleIsConnected}
            </Typography> 
          </Grid>
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.vehicleIsCharging}
            </Typography> 
          </Grid>                                
       
          <Grid item xs={8}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.maxCurrent}
            </Typography>     
          </Grid>
          <Grid item xs={4}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.appliedMaxCurrent}
            </Typography>     
          </Grid> */}
            
        </Grid>            
      </DashboardCard>
    </React.Fragment>
  )
};

export interface ISessionInfoProps {
  socketnr: number;
}
export function EVSESessionInfo(props: ISessionInfoProps) {
  const classes = useStyles();
  const dispatch = useAppDispatch();
  const sessionInfo = useAppSelector(selectSessionInfo);  
  const socketInfo = useAppSelector(selectSocketInfo);  

  useEffect(() => {
    const interval = setInterval(() => {
      // hmmm object arg...
      dispatch(getSessionInfoAsync({id:1}));
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return(
    <React.Fragment>
      <DashboardCard 
        title="Alfen Eve Single Pro-line"
        subheader={'ALF-0000307 Socket #' + props.socketnr }
        avatar={
          <Avatar>
            <EvStationIcon />
          </Avatar>
        }
      >
        <Grid container item xs={12} spacing={1}>
          {socketInfo?.vehicleIsConnected &&
            <Grid item xs={6}>            
              <Typography className={classes.pos}  variant="body2" component="p">
                { sessionInfo?.start } <br/>
                { socketInfo?.mode3StateMessage } { socketInfo?.lastChargingStateChangedFormatted }<br/>
                { socketInfo?.phases == 1 ? "1 phase" : "3 phases" } {socketInfo?.powerAvailable}<br/>  
                { sessionInfo?.energyDelivered } {sessionInfo?.chargingTime}
              </Typography>          
            </Grid>          
          }
          {!socketInfo?.vehicleIsConnected &&
            <Grid item xs={6}>            
              <Typography className={classes.pos}  variant="body2" component="p">
                Available ({ socketInfo?.lastChargingStateChangedFormatted })
              </Typography>          
            </Grid>          
          }
            
 
          {/* <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.maxCurrent} A
            </Typography>
          </Grid>   
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              {socketInfo?.appliedMaxCurrent} A
            </Typography>
          </Grid>     
          <Grid item xs={6}>
            <Typography className={classes.pos} color="textSecondary">
              ?
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
              { sessionInfo?.energyDelivered } kWh
            </Typography>
          </Grid>  
          <Grid item xs={12}>
            <Typography className={classes.pos} color="textSecondary">
              { sessionInfo?.chargingTimeFormatted }
            </Typography>
          </Grid>                                     */}
        </Grid>
      </DashboardCard>
    </React.Fragment>
  )
};
