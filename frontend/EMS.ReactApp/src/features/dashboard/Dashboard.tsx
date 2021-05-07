
import Box from '@material-ui/core/Box';
import Container from '@material-ui/core/Container';
import Grid from '@material-ui/core/Grid';
import Button from '@material-ui/core/Button';

import { useAppSelector, useAppDispatch } from '../../App/hooks';
import { pingAsync } from '../authentication/authenticationSlice';

export default function Main() {
  const isLoggedIn = useAppSelector( state => state.authentication.isLoggedIn ) as boolean;
  const dispatch = useAppDispatch();

  async function onPing(){   
    dispatch(pingAsync()).then((x) =>{

    });
  }

  return(
    <Container>
        <Grid container spacing={1}>
            {isLoggedIn && 
            <Grid item xs={12} sm={4}>
                <Box bgcolor="success.main" color="primary.contrastText" p={2}>
                    hoppa we zijn er in...
                </Box>
                <Button onClick={onPing} >ping</Button>
            </Grid>
            }
            {!isLoggedIn && 
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
