
import React from 'react';
import { styled } from '@mui/material/styles';

import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Grid from '@mui/material/Grid';

import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';


import { useAppSelector, useAppDispatch } from  '../../app/hooks';
import { pingAsync, selectIsLoggedIn } from '../authentication/authenticationSlice';
import { DashboardCard } from '../../components/dashboardcard/DashboardCard';

import { InfoWidget as EVSEInfoWidget } from '../chargepoint/InfoWidget';
import { SocketInfoWidget as EVSESocketInfoWidget } from '../chargepoint/SocketInfoWidget';
import { SessionInfoWidget as EVSESessionInfoWidget } from '../chargepoint/SessionInfoWidget';

const PREFIX = 'Dashboard';
const classes = {
  root: `${PREFIX}-root`,
  paper: `${PREFIX}-paper`,
  pos: `${PREFIX}-pos`,
  bullet: `${PREFIX}-bullet`,
  title: `${PREFIX}-title`,
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.root}`]: {
    flexGrow: 1,
  },
  [`& .${classes.paper}`]: {
    padding: theme.spacing(2),
    textAlign: "center",
    color: theme.palette.text.secondary,
  },
  [`& .${classes.pos}`]: {
    marginBottom: 12,
  },  
  [`& .${classes.bullet}`]: {
    display: 'inline-block',
    margin: '0 2px',
    transform: 'scale(0.8)',
  },
  [`& .${classes.title}`]: {
    fontSize: 14,
  },
}))

export default function Main() {
  const isLoggedIn = useAppSelector(selectIsLoggedIn);
  const dispatch = useAppDispatch();
  const bull = <span className={classes.bullet}>•</span>;

  async function onPing(){   
    dispatch(pingAsync()).then((_x) =>{
    });
  }

  return(
    <Root>
      <Container>
        {isLoggedIn && 
          <Grid container spacing={3} >

            <Grid item xs={12} sm={12} md={6} lg={3} >
              <EVSEInfoWidget />
            </Grid>

            <Grid item xs={12} sm={12} md={6} lg={3} >
              <EVSESocketInfoWidget socketnr={1}/>
            </Grid>

            <Grid item xs={12} sm={12} md={6} lg={3} >
              <EVSESessionInfoWidget socketnr={1}/>
            </Grid>
            
            <Grid item xs={12} sm={12} md={12} lg={3} >
              <DashboardCard title="tst">
                <Grid container spacing={1}>
                  <Grid item xs={6}>
                    <Paper className={classes.paper}>één</Paper>
                  </Grid>
                  <Grid item xs={6}>
                    <Paper className={classes.paper}>twee</Paper>
                  </Grid>              
                  <Grid item xs={12}>
                    <Button aria-label="ping" onClick={onPing} >ping</Button>
                  </Grid>
                </Grid>
              </DashboardCard>
            </Grid>

            <Grid item xs={3} >
              <DashboardCard title="1" >
                <Typography className={classes.title} color="textSecondary" gutterBottom>
                    Word of the Day
                </Typography>
                <Typography variant="h5" component="h2">
                    be{bull}nev{bull}o{bull}lent
                </Typography>
                <Typography className={classes.pos} color="textSecondary">
                    adjective
                </Typography>
                <Typography variant="body2" component="p">
                    well meaning and kindly.
                    <br />
                    {'"a benevolent smile"'}
                </Typography>
              </DashboardCard>
            </Grid>
            <Grid item xs={3} >
              <DashboardCard title="2">b</DashboardCard>
            </Grid>

          </Grid>
        }

        {!isLoggedIn && 
          <Grid item xs={12} sm={4}>
            <>
              <Box bgcolor="primary.main" color="primary.contrastText" p={2}>
                  voor iedereen!yihkkk
              </Box>
            </>
          </Grid>
        } 
      </Container>
    </Root>
  )
}
