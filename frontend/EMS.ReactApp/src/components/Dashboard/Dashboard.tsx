import React from 'react';
import Box from '@material-ui/core/Box';
import Container from '@material-ui/core/Container';
import Grid from '@material-ui/core/Grid';
import useTheme from '@material-ui/core/styles/useTheme';

import { useAppSelector, useAppDispatch } from '../../App/hooks';

import { isLoggedIn } from '../../App/authenticationSlice';

export default function Main() {
  //const theme = useTheme();
  const loggedIn = useAppSelector(isLoggedIn);
  return(
    <Container>
        <Grid container spacing={1}>
            {loggedIn && 
            <Grid item xs={12} sm={4}>
                <Box bgcolor="success.main" color="primary.contrastText" p={2}>
                    hoppa we zijn er in...
                </Box>
            </Grid>
            }
            {!loggedIn && 
            <Grid item xs={12} sm={4}>
                <Box bgcolor="primary.main" color="primary.contrastText" p={2}>
                    voor iedereen!yihkkk
                </Box>
            </Grid>
            }            
        </Grid>
    </Container>
  )
}
