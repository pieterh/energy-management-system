import React, { useEffect } from 'react';

import { makeStyles, createStyles, Theme } from '@material-ui/core/styles';
import Avatar from '@material-ui/core/Avatar';
import Grid from '@material-ui/core/Grid';
import Typography from '@material-ui/core/Typography';
import EvStationIcon from '@material-ui/icons/EvStation';

import { useAppSelector, useAppDispatch } from '../../common/hooks';
import { getSessionInfoAsync, selectSessionInfo, selectSocketInfo } from './EVSESlice';

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


export interface ISessionInfoWidget {
  socketnr: number;
}

export default SessionInfoWidget;
export function SessionInfoWidget(props: ISessionInfoWidget) {
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
            <Grid item xs={12}>            
              <Typography className={classes.pos}  variant="body2" component="p">
                Session started at { sessionInfo?.startFormatted } <br/>
                { socketInfo?.mode3StateMessage } { socketInfo?.lastChargingStateChangedFormatted }<br/>
                { socketInfo?.phases == 1 ? "1 phase" : "3 phases" } {socketInfo?.powerAvailableFormatted}<br/>  
                { sessionInfo?.energyDeliveredFormatted } {sessionInfo?.chargingTime}
              </Typography>          
            </Grid>          
          }
          {!socketInfo?.vehicleIsConnected &&
            <Grid item xs={12}>            
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
