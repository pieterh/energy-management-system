import React, { useEffect } from 'react';
import { getHemsInfoAsync, selectHemsInfo } from './hemsSlice'
import { useAppDispatch, useAppSelector } from  '../../common/hooks';

export function Page() {
  var dispatch = useAppDispatch();
  var hemsInfo = useAppSelector(selectHemsInfo);

  useEffect(() => {
    dispatch(getHemsInfoAsync());
  }, []);

  return(    
      <>
      <h1>{hemsInfo.state}</h1>
      </>
  )
}

export default Page;
