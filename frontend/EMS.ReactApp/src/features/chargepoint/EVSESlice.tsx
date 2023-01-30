import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import deepEqual from "deep-equal";

import { RootState } from "../../app/hooks";

import { FormatFromISODiffNow, FormatDurationFromSeconds, FormatFromISO } from "../../common/DateTimeUtils";

import axios from "axios";

// {
//   "socketInfo": {
//     "id": 1,
//     "voltageFormatted": "229.5 V",
//     "currentFormatted": "48.3 A",
//     "realPowerSumFormatted": "1056964.5 kW",
//     "realEnergyDeliveredFormatted": "1199 kW",
//     "availability": true,
//     "mode3State": "C2",
//     "mode3StateMessage": "Charging",
//     "lastChargingStateChanged": "2021-05-14T22:34:24.411325+02:00",
//     "vehicleIsConnected": true,
//     "vehicleIsCharging": true,
//     "appliedMaxCurrent": "16.0",
//     "maxCurrentValidTime": 0,
//     "maxCurrent": "16.0",
//     "activeLBSafeCurrent": "16",
//     "setPointAccountedFor": true,
//     "phases": 3,
//     "powerAvailableFormatted": "11.0 kW",
//     "powerUsingFormatted": "11.1 kW"
//   },
//   "sessionInfo": {
//     "start": "2021-05-14T22:34:24.411446+02:00",
//     "chargingTime": 0,
//     "energyDeliveredFormatted": "0.0 kWh"
//   },
//   "status": 200,
//   "statusText": null,
//   "message": null
// }

export interface EVSEState {
  socketInfo: SocketInfo;
  sessionInfo: SessionInfo;
}

export interface SocketInfo {
  id: number;
  voltageFormatted: number;
  currentFormatted: number;
  realEnergyDeliveredFormatted: number;
  availability: boolean;
  mode3State: string;
  mode3StateMessage: string;
  lastChargingStateChanged: string | undefined;
  lastChargingStateChangedFormatted: string | undefined;
  vehicleIsConnected: boolean;
  vehicleIsCharging: boolean;
  phases: number;
  appliedMaxCurrent: number;
  maxCurrent: number;
  powerAvailableFormatted: number;
  powerUsingFormatted: number;
}

export interface SessionInfo {
  start: string | undefined;
  startFormatted: string | undefined;
  chargingTime: number | undefined;
  chargingTimeFormatted: string | undefined;
  energyDeliveredFormatted: string | undefined;
}

function CreateState(): EVSEState {
  var newState: EVSEState = {
    socketInfo: {
      id: 0,
      voltageFormatted: 0.0,
      currentFormatted: 0.0,
      realEnergyDeliveredFormatted: 0,
      availability: false,
      mode3State: "E",
      mode3StateMessage: "No Power (E)",
      lastChargingStateChanged: undefined,
      lastChargingStateChangedFormatted: undefined,
      vehicleIsConnected: false,
      vehicleIsCharging: false,
      phases: 0,
      appliedMaxCurrent: 0,
      maxCurrent: 0,
      powerAvailableFormatted: 0,
      powerUsingFormatted: 0,
    },
    sessionInfo: {
      start: undefined,
      startFormatted: undefined,
      chargingTime: undefined,
      chargingTimeFormatted: undefined,
      energyDeliveredFormatted: undefined,
    },
  };
  return newState;
}

const initialState = CreateState();

function UpdateStateSessionInfo(state: EVSEState, sr: SocketInfoResponse) {
  var si: SocketInfo = {
    ...sr.socketInfo,
    lastChargingStateChangedFormatted: FormatFromISODiffNow(sr.socketInfo.lastChargingStateChanged),
  };

  var ses: SessionInfo = {
    ...sr.sessionInfo,
    startFormatted: !!sr.sessionInfo ? FormatFromISO(sr.sessionInfo.start) : undefined,
    chargingTimeFormatted: !!sr.sessionInfo?.chargingTime
      ? FormatDurationFromSeconds(sr.sessionInfo?.chargingTime)
      : undefined,
  };

  // validate if the data is really updated before updating the state
  // preventing updates that are not needed
  if (!deepEqual(state.socketInfo, si)) {
    state.socketInfo = si;
    console.log("new data 1;-)");
  }
  if (!deepEqual(state.sessionInfo, ses)) {
    state.sessionInfo = ses;
    console.log("new data 2;-)");
  }
}

interface Response {
  status: number;
  statusText: string;
  message: string;
}

interface SocketInfoResponse extends Response {
  socketInfo: SocketR;
  sessionInfo: SessionR;
}

export interface SessionR {
  start: string;
  chargingTime: number;
  energyDeliveredFormatted: string;
}

export interface SocketR {
  id: number;
  voltageFormatted: number;
  currentFormatted: number;
  realEnergyDeliveredFormatted: number;
  availability: boolean;
  mode3State: string;
  mode3StateMessage: string;
  lastChargingStateChanged: string | undefined;
  vehicleIsConnected: boolean;
  vehicleIsCharging: boolean;
  phases: number;
  appliedMaxCurrent: number;
  maxCurrent: number;
  powerAvailableFormatted: number;
  powerUsingFormatted: number;
}

export const getSessionInfoAsync = createAsyncThunk<SocketInfoResponse, { id: number }, { rejectValue: Response }>(
  "evse/getSession",
  async ({ id }: { id: number }, { rejectWithValue }) => {
    try {
      var cfg = undefined;
      const apiBaseURL = window.location.protocol + "//" + window.location.hostname + ":" + window.location.port + "/api";
      var response = await axios.get<SocketInfoResponse>(apiBaseURL + `/evse/socket/${id}`, cfg);
      return response.data;
    } catch (err) {
      return rejectWithValue({
        status: err.response.status,
        statusText: err.response.statusText,
        message: err.resonse.message,
      });
    }
  }
);

export const evseSlice = createSlice({
  name: "evse",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(getSessionInfoAsync.fulfilled, (state, action) => {
        UpdateStateSessionInfo(state, action.payload);
      })
      .addCase(getSessionInfoAsync.rejected, (state, action) => {
        console.info(`getSessionInfoAsync rejected - ${action.payload?.status} - ${action.payload?.statusText}`);
      });
  },
});

export default evseSlice.reducer;

export const selectSocketInfo = (state: RootState) => state.evse.socketInfo;
export const selectSessionInfo = (state: RootState) => state.evse.sessionInfo;

export const vehicleIsConnected = (state: RootState) => state.evse.socketInfo.vehicleIsConnected;
export const vehicleIsCharging = (state: RootState) => state.evse.socketInfo.vehicleIsCharging;
