import React, { useEffect } from "react";

import { makeStyles, createStyles, Theme } from "@material-ui/core/styles";
import Avatar from "@material-ui/core/Avatar";
import Grid from "@material-ui/core/Grid";
import Typography from "@material-ui/core/Typography";

import { useAppSelector, useAppDispatch } from "../../common/hooks";
import { getHemsInfoAsync, selectHemsInfo } from "./hemsSlice";
import { vehicleIsConnected } from "../chargepoint/EVSESlice";
import { DashboardCard } from "../../components/dashboardcard/DashboardCard";
import EvStationIcon from "@material-ui/icons/EvStation";

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      flexGrow: 1,
    },
    paper: {
      padding: theme.spacing(2),
      textAlign: "center",
      color: theme.palette.text.secondary,
    },
    pos: {
      marginBottom: 12,
    },
  })
);

export interface IChargingInfoWidget {}

export default ChargingInfoWidget;
export function ChargingInfoWidget(props: IChargingInfoWidget) {
  const classes = useStyles();
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
    </React.Fragment>
  );
}
