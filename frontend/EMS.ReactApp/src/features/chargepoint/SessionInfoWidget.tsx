import React, { useEffect } from "react";
import { styled } from '@mui/material/styles';

import Avatar from '@mui/material/Avatar';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import EvStationIcon from '@mui/icons-material/EvStation';

import { useAppSelector, useAppDispatch } from "../../app/hooks";
import { getSessionInfoAsync, selectSessionInfo, selectSocketInfo } from "./EVSESlice";

import { DashboardCard } from "../../components/dashboardcard/DashboardCard";

const PREFIX = 'SessionInfoWidget';
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

export interface ISessionInfoWidget {
  socketnr: number;
}

export default SessionInfoWidget;
export function SessionInfoWidget(props: ISessionInfoWidget) {
  const dispatch = useAppDispatch();
  const sessionInfo = useAppSelector(selectSessionInfo);
  const socketInfo = useAppSelector(selectSocketInfo);

  useEffect(() => {
    const interval = setInterval(() => {
      // hmmm object arg...
      dispatch(getSessionInfoAsync({ id: 1 }));
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <React.Fragment>
      <Root>
        <DashboardCard
          title="Alfen Eve Single Pro-line"
          subheader={"ALF-0000307 Socket #" + props.socketnr}
          avatar={
            <Avatar>
              <EvStationIcon />
            </Avatar>
          }
        >
          <Grid container item xs={12} spacing={1}>
            {socketInfo?.vehicleIsConnected && (
              <Grid item xs={12}>
                <Typography className={classes.pos} variant="body2" component="p">
                  Session started at {sessionInfo?.startFormatted} <br />
                  Capacity available {socketInfo?.powerAvailableFormatted} ({socketInfo?.phases == 1 ? "1 phase" : "3 phases"})
                  <br />
                  {socketInfo?.mode3StateMessage} {socketInfo?.lastChargingStateChangedFormatted}
                  <br />
                  Charged {sessionInfo?.energyDeliveredFormatted} (in {sessionInfo?.chargingTimeFormatted})
                </Typography>
              </Grid>
            )}
            {!socketInfo?.vehicleIsConnected && (
              <Grid item xs={12}>
                <Typography className={classes.pos} variant="body2" component="p">
                  Available ({socketInfo?.lastChargingStateChangedFormatted})
                </Typography>
              </Grid>
            )}
          </Grid>
        </DashboardCard>
      </Root>
    </React.Fragment>
  );
}
