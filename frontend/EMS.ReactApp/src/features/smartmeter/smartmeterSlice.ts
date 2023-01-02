import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import deepEqual from "deep-equal";

import { RootState } from "../../common/hooks";

import { FormatFromISODiffNow, FormatDurationFromSeconds, FormatFromISO } from "../../common/DateTimeUtils";

import axios from "axios";

// {
//     "info": {
//         "currentL1": "0.0 A",
//         "currentL2": "1.2 A",
//         "currentL3": "0.3 A",
//         "voltageL1": "234.0 V",
//         "voltageL2": "233.6 V",
//         "voltageL3": "232.6 V",
//         "tariffIndicator": 1,
//         "electricity1FromGrid": "2861 kWh",
//         "electricity1ToGrid": "1064 kWh",
//         "electricity2FromGrid": "2081 kWh",
//         "electricity2ToGrid": "2393 kWh"
//     },
//     "status": 0,
//     "statusText": null,
//     "message": null
// }

export interface SmartMeterState {
  smartmeterInfo: smartmeterInfo;
}

export interface smartmeterInfo {
  currentL1Formatted: string;
  currentL2Formatted: string;
  currentL3Formatted: string;
  voltageL1Formatted: string;
  voltageL2Formatted: string;
  voltageL3Formatted: string;
  electricity1FromGrid: string;
  electricity1ToGrid: string;
  electricity2FromGrid: string;
  electricity2ToGrid: string;
}

function CreateState(): SmartMeterState {
  var newState: SmartMeterState = {
    smartmeterInfo: {
        currentL1Formatted: "-.- A",
        currentL2Formatted: "-.- A",
        currentL3Formatted: "-.- A",
        voltageL1Formatted: "---.- V",
        voltageL2Formatted: "---.- V",
        voltageL3Formatted: "---.- V",
        electricity1FromGrid: "0 kWh",
        electricity1ToGrid: "0 kWh",
        electricity2FromGrid: "0 kWh",
        electricity2ToGrid: "0 kWh"
    },
  };
  return newState;
}

const initialState = CreateState();

function UpdateStateSessionInfo(state: SmartMeterState, sr: smartmeterInfoResponse) {
  var hi: smartmeterInfo = {
    ...sr.info,
  };

  // validate if the data is really updated before updating the state
  // preventing updates that are not needed
  if (!deepEqual(state.smartmeterInfo, hi)) {
    state.smartmeterInfo = hi;
    console.log("new data 1;-)");
  }
}

interface Response {
  status: number;
  statusText: string;
  message: string;
}

interface smartmeterInfoResponse extends Response {
  info: smartmeterR;
}

export interface smartmeterR {
    currentL1Formatted: string;
    currentL2Formatted: string;
    currentL3Formatted: string;
    voltageL1Formatted: string;
    voltageL2Formatted: string;
    voltageL3Formatted: string;
    electricity1FromGrid: string;
    electricity1ToGrid: string;
    electricity2FromGrid: string;
    electricity2ToGrid: string;
}

export const getSmartMeterInfoAsync = createAsyncThunk<smartmeterInfoResponse, undefined, { rejectValue: Response }>(
  "smartmeter/getInfo",
  async (undefined, { rejectWithValue }) => {
    try {
      var cfg = undefined;
      const apiBaseURL = window.location.protocol + "//" + window.location.hostname + ":" + window.location.port + "/api";
      var response = await axios.get<smartmeterInfoResponse>(apiBaseURL + `/smartmeter/info/`, cfg);
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

export const smartmeterSlice = createSlice({
  name: "smartmeter",
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(getSmartMeterInfoAsync.fulfilled, (state, action) => {
        UpdateStateSessionInfo(state, action.payload);
      })
      .addCase(getSmartMeterInfoAsync.rejected, (state, action) => {
        console.info(`getSmartMeterInfoAsync rejected - ${action.payload?.status} - ${action.payload?.statusText}`);
      });
  },
});

export default smartmeterSlice.reducer;

export const selectSmartMeterInfo = (state: RootState) => state.smartmeter.smartmeterInfo;
