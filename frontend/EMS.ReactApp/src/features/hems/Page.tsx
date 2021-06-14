import React, { useEffect } from "react";
import { getHemsInfoAsync, selectHemsInfo } from "./hemsSlice";
import { useAppDispatch, useAppSelector } from "../../common/hooks";
import { ChargingInfoWidget } from "./ChargeInfoWidget";
import { GraphWidget } from "./GraphWidget";

export function Page() {
  var dispatch = useAppDispatch();
  var hemsInfo = useAppSelector(selectHemsInfo);

  useEffect(() => {
    dispatch(getHemsInfoAsync());
  }, []);

  return (
    <>
      <ChargingInfoWidget />
      <GraphWidget />
    </>
  );
}

export default Page;
