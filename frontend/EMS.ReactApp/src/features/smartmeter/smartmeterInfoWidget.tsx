import React, { useEffect } from "react";
import { styled } from '@mui/material/styles';

import Avatar from '@mui/material/Avatar';
import Container from '@mui/material/Container';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';

import { useAppSelector, useAppDispatch } from "../../common/hooks";
import { getSmartMeterInfoAsync, selectSmartMeterInfo } from "./smartmeterSlice";
import { vehicleIsConnected } from "../chargepoint/EVSESlice";
import { DashboardCard } from "../../components/dashboardcard/DashboardCard";
import ElectricalServices from "../../icons/ElectricalServices";

const PREFIX = 'smartmeterInfoWidget';
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

export interface ISmartMeterInfoWidget {}

export default SmartMeterInfoWidget;
export function SmartMeterInfoWidget(props: ISmartMeterInfoWidget) {
  const dispatch = useAppDispatch();
  const smartmeterInfo = useAppSelector(selectSmartMeterInfo);

  useEffect(() => {
    const interval = setInterval(() => {
      dispatch(getSmartMeterInfoAsync());
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return (    
    <Root>
      <Container>
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
      </Container>
    </Root>
  );
}
