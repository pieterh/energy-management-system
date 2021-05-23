import React, { useEffect } from "react";

import { makeStyles, createStyles, Theme } from "@material-ui/core/styles";
import Avatar from "@material-ui/core/Avatar";
import Grid from "@material-ui/core/Grid";
import Typography from "@material-ui/core/Typography";

import { useAppSelector, useAppDispatch } from "../../common/hooks";
import { getSmartMeterInfoAsync, selectSmartMeterInfo } from "./smartmeterSlice";
import { vehicleIsConnected } from "../chargepoint/EVSESlice";
import { DashboardCard } from "../../components/dashboardcard/DashboardCard";
import ElectricalServices from "../../icons/ElectricalServices";

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

export interface ISmartMeterInfoWidget {}

export default SmartMeterInfoWidget;
export function SmartMeterInfoWidget(props: ISmartMeterInfoWidget) {
  const classes = useStyles();
  const dispatch = useAppDispatch();
  const smartmeterInfo = useAppSelector(selectSmartMeterInfo);

  useEffect(() => {
    const interval = setInterval(() => {
      dispatch(getSmartMeterInfoAsync());
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <React.Fragment>
      <DashboardCard
        title="Smart Meter"
        subheader={"uh"}
        avatar={
          <Avatar>
            <ElectricalServices />
          </Avatar>
        }
      >
        <Grid container item xs={12} spacing={1}>
          <Grid item xs={12}>
            <Typography className={classes.pos} variant="body2" component="p">
              {smartmeterInfo.electricity1FromGrid} {smartmeterInfo.electricity1ToGrid}
              <br />
              {smartmeterInfo.electricity2FromGrid} {smartmeterInfo.electricity1ToGrid}
              <br />
              <br />
            </Typography>
          </Grid>
        </Grid>
      </DashboardCard>
    </React.Fragment>
  );
}
