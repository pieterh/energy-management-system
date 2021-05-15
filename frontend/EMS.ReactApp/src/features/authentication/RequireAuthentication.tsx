import React, { useState, useEffect } from 'react';

import { useHistory } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from  '../../common/hooks';
import { selectIsLoggedIn } from '../authentication/authenticationSlice';

// Mark the function as a generic using P (or whatever variable you want)
// export function RequireAuthentication<P>(
//   WrappedComponent: React.ComponentType<P>
// ) {
//   // const isLoggedIn = useAppSelector(selectIsLoggedIn);
//   // const history = useHistory();
//   // if (!isLoggedIn){
//   //   console.log("Redirect to logout");
//   //   history.push('/logout');
//   //   return (<></>);
//   // }

//   const ComponentWithExtraInfo = (props: P) => {
//     // At this point, the props being passed in are the original props the component expects.
//     return <WrappedComponent  />;
//   };
//   return ComponentWithExtraInfo;
// }

// export default RequireAuthentication;




export default function RequireAuthentication() {
  const isLoggedIn = useAppSelector(selectIsLoggedIn);
  const history = useHistory();
  useEffect(() => {  
    if (!isLoggedIn){
      console.log("Redirect to login");
      history.push('/login');

    }
  });
  return(<></>);
}

