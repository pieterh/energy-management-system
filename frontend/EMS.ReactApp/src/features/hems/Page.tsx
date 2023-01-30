import React, { useEffect } from "react";
import Container from '@mui/material/Container';
import { getHemsInfoAsync, selectHemsInfo } from "./hemsSlice";
import { useAppDispatch, useAppSelector } from "../../app/hooks";
import { ChargingInfoWidget } from "./ChargeInfoWidget";
import { GraphWidget } from "./GraphWidget";

export function Page() {
  var dispatch = useAppDispatch();
  var hemsInfo = useAppSelector(selectHemsInfo);

  useEffect(() => {
    dispatch(getHemsInfoAsync());
  }, []);

  return (
    <Container>
      <ChargingInfoWidget />
      <GraphWidget />
    </Container>
  );
}

export default Page;
