import { createSlice } from '@reduxjs/toolkit';
import { RootState } from '../../common/hooks';

export interface IDrawerState {
    isOpen : boolean
  }

const initialState: IDrawerState = {
    isOpen: false
  };

export const drawerSlice = createSlice({
    name: 'drawer',
    initialState,
    reducers: {
        openDrawer(state) {
            state.isOpen = true;
        },
        closeDrawer(state) {
            state.isOpen = false;
        },
        toggleDrawer(state) {
            state.isOpen = !state.isOpen;
        },
    },
  });

export default drawerSlice.reducer;
export const { openDrawer, closeDrawer, toggleDrawer } = drawerSlice.actions;

// some selectors to access data from the store that is managed by this slice
export const selectIsDrawerOpen = (state : RootState) => state.drawer.isOpen;
