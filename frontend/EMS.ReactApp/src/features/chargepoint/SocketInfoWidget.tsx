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

export interface ISocketInfoProps {
  socketnr: number;
}

export default SocketInfoWidget;
export function SocketInfoWidget(props: ISocketInfoProps) {
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
              { socketInfo?.realEnergyDeliveredFormatted } kWh energy delivered<br/>             
              { socketInfo?.powerAvailableFormatted } <br/>
              { socketInfo?.vehicleIsConnected && <> { socketInfo?.powerUsingFormatted } </> }
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

