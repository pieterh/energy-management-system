import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import  deepEqual  from 'deep-equal';

import { RootState } from '../../common/hooks';

import { FormatFromISODiffNow, FormatDurationFromSeconds } from '../../common/DateTimeUtils';



import axios from 'axios';

// {
//   "socketInfo": {
//     "id": 1,
//     "voltage": 237.6,
//     "current": 48.1,
//     "realEnergyDelivered": 0,
//     "availability": true,
//     "mode3State": "C2",
//     "mode3StateMessage": "Charging",
//     "lastChargingStateChanged": "2021-05-14T13:38:55.722098+02:00",
//     "vehicleIsConnected": true,
//     "vehicleIsCharging": true,
//     "appliedMaxCurrent": 16,
//     "maxCurrentValidTime": 0,
//     "maxCurrent": 16,
//     "activeLBSafeCurrent": 16,
//     "setPointAccountedFor": true,
//     "phases": 3
//   },
//   "sessionInfo": {
//     "start": "2021-05-14T13:38:07.06664+02:00",
//     "chargingTime": 0,
//     "energyDelivered": 0.33
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
  id: number,
  voltage: number;
  current: number;
  realEnergyDelivered: number;
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
  powerAvailable: number;
  powerUsing: number;
}  

export interface SessionInfo {
  start: string | undefined;
  chargingTime: number | undefined;
  chargingTimeFormatted: string | undefined;
  energyDelivered: number | undefined;
}

function  CreateState() : EVSEState {
  var newState : EVSEState = { 
    socketInfo : {
      "id": 0,
      "voltage": 0.0,
      "current": 0.0,
      "realEnergyDelivered": 0,
      "availability": false,  
      "mode3State": "E",
      "mode3StateMessage": "No Power (E)",
      "lastChargingStateChanged": undefined,
      "lastChargingStateChangedFormatted": undefined,
      "vehicleIsConnected": false,
      "vehicleIsCharging": false,
      "phases": 0,
      "appliedMaxCurrent": 0,
      "maxCurrent": 0,
      "powerAvailable": 0,
      "powerUsing": 0,
    },
    sessionInfo : {
      "start": undefined,
      "chargingTime": undefined,
      "chargingTimeFormatted": undefined,
      "energyDelivered": undefined
    }
  };
  return newState;
}

const initialState = CreateState();

function UpdateStateSessionInfo(state: EVSEState, sr: SocketInfoResponse) {
    var si : SocketInfo = {
      ...sr.socketInfo, 
      lastChargingStateChangedFormatted: FormatFromISODiffNow(sr.socketInfo.lastChargingStateChanged)
    };

    var ses : SessionInfo = {
      ...sr.sessionInfo,
      chargingTimeFormatted: !!sr.sessionInfo?.chargingTime ? FormatDurationFromSeconds(sr.sessionInfo?.chargingTime) : undefined
    }

    // validate if the data is really updated before updating the state
    // preventing updates that are not needed
    if (!deepEqual(state.socketInfo, si) ){
        state.socketInfo = si;
        console.log("new data 1;-)"); 
    }
    if (!deepEqual(state.sessionInfo, ses) ){
      state.sessionInfo = ses;
      console.log("new data 2;-)"); 
  }  
}

interface Response {
  status: number,
  statusText: string,
  message: string  
}

interface SocketInfoResponse extends Response {
  socketInfo: SocketR;
  sessionInfo: SessionR;
}

export interface SessionR {
  start: string;
  chargingTime: number;
  energyDelivered: number;
}

export interface SocketR {
  id: number,
  voltage: number,
  current: number,
  realEnergyDelivered: number;
  availability: boolean;  
  mode3State: string;
  mode3StateMessage: string;
  lastChargingStateChanged: string | undefined;
  vehicleIsConnected: boolean;
  vehicleIsCharging: boolean;
  phases: number;
  appliedMaxCurrent: number;
  maxCurrent: number;
  powerAvailable: number;
  powerUsing: number;
}  


export const getSessionInfoAsync = createAsyncThunk<
      SocketInfoResponse, 
      {id: number},
      {rejectValue: Response}
    >(
    'evse/getSession',
    async ({id}: {id: number}, { rejectWithValue }) => {
      try {
        var cfg = undefined;
        var response = await axios.get<SocketInfoResponse>(`http://127.0.0.1:5000/api/evse/socket/${id}`, cfg);
        return response.data;
      }catch(err){        
        return rejectWithValue(
          {
            status: err.response.status, 
            statusText: err.response.statusText, 
            message: err.resonse.message
          });
      }
    }
  );

export const evseSlice = createSlice({
    name: 'evse',
    initialState,
    reducers: {
    },
    extraReducers: (builder) => {
      builder
        .addCase(getSessionInfoAsync.fulfilled, (state, action) => {    
            UpdateStateSessionInfo(state, action.payload);
        })
        .addCase(getSessionInfoAsync.rejected, (state, action ) => {
          console.info(`getSessionInfoAsync rejected - ${action.payload?.status} - ${action.payload?.statusText}`);  
        })
        ;
    },
  });

export default evseSlice.reducer;      



export const selectSocketInfo = (state : RootState) => state.evse.socketInfo;
export const selectSessionInfo = (state : RootState) => state.evse.sessionInfo;
