import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import deepEqual from "deep-equal";

import { RootState } from "../../common/hooks";

import { FormatFromISODiffNow, FormatDurationFromSeconds, FormatFromISO } from "../../common/DateTimeUtils";

import axios from "axios";

// {
//   "info": {
//     "mode": "MaxSolar",
//     "state": "NotCharging",
//     "lastStateChange": "2021-05-23T16:30:53.478223+02:00",
//     "currentAvailableL1Formatted": "0.0 A",
//     "currentAvailableL2Formatted": "0.0 A",
//     "currentAvailableL3Formatted": "0.0 A"
//   },
//   "status": 0,
//   "statusText": null,
//   "message": null
// }

export interface HemsState {
  hemsInfo: hemsInfo;
  measurements: hemsMeasurement[];
}

export interface hemsInfo {
  mode: string;
  state: string;
  lastStateChangeFormatted: string;
  currentAvailableL1Formatted: string;
  currentAvailableL2Formatted: string;
  currentAvailableL3Formatted: string;
}

export interface hemsMeasurement {
  received: string;
  l1 : number;
  l2 : number;
  l3 : number;
  l : number;
  cL1 : number;
  cL2 : number;
  cL3 : number;
}

function CreateState(): HemsState {
  var newState: HemsState = {
    hemsInfo: {
      mode: "",
      state: "",
      lastStateChangeFormatted: "",
      currentAvailableL1Formatted: "",
      currentAvailableL2Formatted: "",
      currentAvailableL3Formatted: "",
    },
    measurements: []
  };
  return newState;
}

const initialState = CreateState();

function UpdateStateSessionInfo(state: HemsState, sr: HemsInfoResponse) {
  var hi: hemsInfo = {
    ...sr.info,     
    lastStateChangeFormatted: FormatFromISO(sr.info.lastStateChange),
  };

  // validate if the data is really updated before updating the state
  // preventing updates that are not needed
  if (!deepEqual(state.hemsInfo, hi)) {
    state.hemsInfo = hi;
    console.log("new data 1;-)");
  }

  state.measurements = sr.measurements;
}

interface Response {
  status: number;
  statusText: string;
  message: string;
}

interface HemsInfoResponse extends Response {
  info: hemsR;
  measurements: hemsMeasurementR[];
}

export interface hemsR {
  mode: string;
  state: string;
  lastStateChange: string;
  currentAvailableL1Formatted: string;
  currentAvailableL2Formatted: string;
  currentAvailableL3Formatted: string;
}

export interface hemsMeasurementR {
  received: string;
  l1 : number;
  l2 : number;
  l3 : number;
  l : number;
  cL1 : number;
  cL2 : number;
  cL3 : number;
}

export const getHemsInfoAsync = createAsyncThunk<HemsInfoResponse, undefined, { rejectValue: Response }>(
  "hems/getInfo",
  async (undefined, { rejectWithValue }) => {
    try {
      var cfg = undefined;
      const apiBaseURL = window.location.protocol + "//" + window.location.hostname + ":" + window.location.port + "/api";
      var response = await axios.get<HemsInfoResponse>(apiBaseURL + `/hems/info/`, cfg);
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

export const hemsSlice = createSlice({
  name: "hems",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(getHemsInfoAsync.fulfilled, (state, action) => {
        UpdateStateSessionInfo(state, action.payload);
      })
      .addCase(getHemsInfoAsync.rejected, (state, action) => {
        console.info(`getHemsInfoAsync rejected - ${action.payload?.status} - ${action.payload?.statusText}`);
      });
  },
});

export default hemsSlice.reducer;

export const selectHemsInfo = (state: RootState) => state.hems.hemsInfo;

export const selectMeasurements = (state: RootState) => state.hems.measurements;
