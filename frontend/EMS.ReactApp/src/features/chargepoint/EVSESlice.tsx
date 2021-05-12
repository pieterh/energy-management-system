import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import  deepEqual  from 'deep-equal';

import { RootState } from '../../common/hooks';

import { FormatFromISODiffNow } from '../../common/DateTimeUtils';



import axios from 'axios';

// {
//   "sessionInfo": {
//     "id": 0,
//     "mode3State": "E",
//     "mode3StateMessage": "No Power (E)",
//     "lastChargingStateChanged": "2021-05-13T16:27:43.856125+02:00",
//     "vehicleIsConnected": false,
//     "vehicleIsCharging": false,
//     "phases": 3,
//     "appliedMaxCurrent": 0,
//     "maxCurrent": 16
//   },
//   "status": 200,
//   "statusText": null,
//   "message": null
// }


export interface EVSEState {
    sessionInfo: SessionInfo;
  }

export interface SessionInfo {
    id: number,
    mode3State: string;
    mode3StateMessage: string;
    lastChargingStateChanged: string | undefined;
    lastChargingStateChangedFormatted: string | undefined;
    vehicleIsConnected: boolean;
    vehicleIsCharging: boolean;
    phases: number;
    appliedMaxCurrent: number;
    maxCurrent: number;
}  

function  CreateState() : EVSEState {
  var newState : EVSEState = { 
    sessionInfo : {
        "id": 0,
        "mode3State": "E",
        "mode3StateMessage": "No Power (E)",
        "lastChargingStateChanged": undefined,
        "lastChargingStateChangedFormatted": undefined,
        "vehicleIsConnected": false,
        "vehicleIsCharging": false,
        "phases": 0,
        "appliedMaxCurrent": 0,
        "maxCurrent": 0
    }
  };
  return newState;
}

const initialState = CreateState();

function UpdateStateSessionInfo(state: EVSEState, sir: SessionInfoR) { 

    var si : SessionInfo = {
      ...sir, 
      lastChargingStateChangedFormatted: FormatFromISODiffNow(sir?.lastChargingStateChanged)
    };

    // validate if the data is really updated before updating the state
    // preventing updates that are not needed
    if (!deepEqual(state.sessionInfo, si) ){        
        state.sessionInfo = si;
        console.log("new data ;-)"); 
    }  
}

interface Response {
  status: number,
  statusText: string,
  message: string  
}

interface SessionInfoResponse extends Response {
  sessionInfo: SessionInfoR
}

export interface SessionInfoR {
  id: number,
  mode3State: string;
  mode3StateMessage: string;
  lastChargingStateChanged: string | undefined;
  vehicleIsConnected: boolean;
  vehicleIsCharging: boolean;
  phases: number;
  appliedMaxCurrent: number;
  maxCurrent: number;
}  


export const getSessionInfoAsync = createAsyncThunk<
      SessionInfoResponse, 
      {id: number},
      {rejectValue: Response}
    >(
    'evse/getSession',
    async ({id}: {id: number}, { rejectWithValue }) => {
      try {
        var cfg = undefined;
        var response = await axios.get<SessionInfoResponse>(`http://127.0.0.1:5000/api/evse/socket/${id}/session`, cfg);
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
            UpdateStateSessionInfo(state, action.payload.sessionInfo);
        })
        .addCase(getSessionInfoAsync.rejected, (state, action ) => {
          console.info(`getSessionInfoAsync rejected - ${action.payload?.status} - ${action.payload?.statusText}`);  
        })
        ;
    },
  });

export default evseSlice.reducer;      



export const selectSessionInfo = (state : RootState) => state.evse.sessionInfo;
