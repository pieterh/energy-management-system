import React, { useEffect } from "react";
import { getHemsInfoAsync, selectHemsInfo } from "./hemsSlice";
import { useAppDispatch, useAppSelector } from "../../common/hooks";
import { ChargingInfoWidget } from "./ChargeInfoWidget";

export function Page() {
  var dispatch = useAppDispatch();
  var hemsInfo = useAppSelector(selectHemsInfo);

  useEffect(() => {
    dispatch(getHemsInfoAsync());
  }, []);

  return (
    <>
      <ChargingInfoWidget />
    </>
  );
}

export default Page;
