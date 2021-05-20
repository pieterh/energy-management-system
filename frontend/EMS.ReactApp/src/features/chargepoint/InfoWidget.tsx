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

export default InfoWidget;
export function InfoWidget() {
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
