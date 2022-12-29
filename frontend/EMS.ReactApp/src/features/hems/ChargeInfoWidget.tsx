import React, { useEffect } from "react";
import { styled } from '@mui/material/styles';

import Avatar from '@mui/material/Avatar';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';

import { useAppSelector, useAppDispatch } from "../../common/hooks";
import { getHemsInfoAsync, selectHemsInfo } from "./hemsSlice";
import { vehicleIsConnected } from "../chargepoint/EVSESlice";
import { DashboardCard } from "../../components/dashboardcard/DashboardCard";
import EvStationIcon from '@mui/icons-material/EvStation';

const PREFIX = 'ChargeInfoWidget';
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

export interface IChargingInfoWidget {}

export default ChargingInfoWidget;
export function ChargingInfoWidget(props: IChargingInfoWidget) {
  const dispatch = useAppDispatch();
  const hemsInfo = useAppSelector(selectHemsInfo);
  const isVehicleConnected = useAppSelector(vehicleIsConnected);

  useEffect(() => {
    const interval = setInterval(() => {
      dispatch(getHemsInfoAsync());
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <React.Fragment>
      <Root>
        <DashboardCard
          title="HEMS"
          subheader={"charge"}
          avatar={
            <Avatar>
              <EvStationIcon />
            </Avatar>
          }
        >
          <Grid container item xs={12} spacing={1}>
            {isVehicleConnected && (
              <Grid item xs={12}>
                <Typography className={classes.pos} variant="body2" component="p">
                  {hemsInfo.mode}
                  <br />
                  {hemsInfo.state} {hemsInfo?.lastStateChangeFormatted} <br />
                  {hemsInfo.currentAvailableL1Formatted} {hemsInfo.currentAvailableL2Formatted}{" "}
                  {hemsInfo.currentAvailableL3Formatted}
                </Typography>
              </Grid>
            )}
            {!isVehicleConnected && (
              <Grid item xs={12}>
                <Typography className={classes.pos} variant="body2" component="p">
                  {hemsInfo.mode}
                  <br />
                  {hemsInfo.state} {hemsInfo?.lastStateChangeFormatted} <br />
                  {hemsInfo.currentAvailableL1Formatted} {hemsInfo.currentAvailableL2Formatted}{" "}
                  {hemsInfo.currentAvailableL3Formatted}
                </Typography>
              </Grid>
            )}
          </Grid>
        </DashboardCard>
      </Root>
    </React.Fragment>
  );
}
