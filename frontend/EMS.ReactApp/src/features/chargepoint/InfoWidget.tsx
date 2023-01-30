import React, { useEffect } from 'react';
import { styled } from '@mui/material/styles';

import Avatar from '@mui/material/Avatar';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import EvStationIcon from '@mui/icons-material/EvStation';

import { useAppSelector, useAppDispatch } from '../../app/hooks';
import { getSessionInfoAsync, selectSessionInfo, selectSocketInfo } from './EVSESlice';

import { DashboardCard } from '../../components/dashboardcard/DashboardCard';

const PREFIX = 'InfoWidget';
const classes = {
  root: `${PREFIX}-root`,
  paper: `${PREFIX}-paper`,
  pos: `${PREFIX}-pos`,
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.root}`]: {
    flexGrow: 1,
  },
  [`& .${classes.paper}`]: {
    padding: theme.spacing(2),
    textAlign: "center",
    color: theme.palette.text.secondary,
  },
  [`& .${classes.pos}`]: {
    marginBottom: 12,
  },
}))

export default InfoWidget;
export function InfoWidget() {
  return(
    <React.Fragment>
      <Root>
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
      </Root>
    </React.Fragment>
  )
};
